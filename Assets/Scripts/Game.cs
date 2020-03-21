using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/*
    manage game states, snapshots, authority control ()
    (tmp) settings:
        8*8 plane
        15*15 cubes
        6 player
*/
[DefaultExecutionOrder(0)]
public class Game : MonoBehaviour {
    public const float visualSmoothCoef = .15f; // Value between 0 and 1, determines how much we smooth the render transform.
    public const float mainPlayerSmoothCoef = .5f;

    public GameObject cubeRenderPrefab;
    public GameObject cubePhysicPrefab;
    public GameObject playerPrefab;
    public GameObject playerRenderPrefab;

    private Snapshot _snapshot; // Always contains all cubes
    private int _mainPlayerId; // should always be 0
    private Transform[] _renderCubeTrans; // TODO: bad style
    private GameObject[] _renderPlayer; // TODO: bad style

    private static Game _instance;
    public static Game Instance { get => _instance; }


    private bool _isStarted;

    private bool useImpluseForce;  // whether add impluse invisible force to the cube
    private bool isServer;


    void Start() {
        if (_instance && _instance != this) {
            Debug.LogError("Singleton Error");
            Destroy(gameObject);
            return;
        }
        Assert.IsNull(_instance);
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _isStarted = false;
        isServer = false;
    }

    public void InitGame(int player, bool isServer) {
        _snapshot = new Snapshot();
        // init scene
        InitSceneCubes(cubePhysicPrefab, cubeRenderPrefab, out _snapshot.cubeStates, out _renderCubeTrans);
        Debug.Log("[Init cubes]" + _snapshot.cubeStates.Length);
        _snapshot.playerStates = InitPlayers(playerPrefab, playerRenderPrefab, out _renderPlayer);
        Debug.Log("[Init players]" + _snapshot.playerStates.Length);
        // init main player
        Assert.IsTrue(_snapshot.playerStates.Length > 1);
        _mainPlayerId = player;
        _snapshot.playerStates[_mainPlayerId].Go.AddComponent<PlayerController>();
        _snapshot.playerStates[_mainPlayerId].SetActive(true);
        _renderPlayer[_mainPlayerId].SetActive(true);

        UpdateSnapshot();

        _isStarted = true;
        this.isServer = isServer;
    }

    void FixedUpdate() {
        if (!_isStarted) return;
        Vector3 ori = _snapshot.playerStates[_mainPlayerId].Position;
        foreach (var rbObj in _snapshot.cubeStates) {
            if (useImpluseForce) {
                rbObj.Rigidbody.AddExplosionForce(0.1f, ori, 3.0f, 0.1f, ForceMode.Impulse);
            }
        }
    }

    public void changeImpulseForceOnCube() {
        useImpluseForce = !useImpluseForce;
    }

    // It will update every cube's position in real time.
    // If the game is not started, it shouldn't run.
    void Update() {
        if (!_isStarted) return;
        for (int i = 0; i < _snapshot.CubeCount; i++) {
            _renderCubeTrans[i].position = Vector3.Lerp(_renderCubeTrans[i].position,
                _snapshot.cubeStates[i].Go.transform.position,
                visualSmoothCoef);
            _renderCubeTrans[i].rotation = Quaternion.Slerp(_renderCubeTrans[i].rotation,
                _snapshot.cubeStates[i].Go.transform.rotation,
                visualSmoothCoef);
        }
        for (int i = 0; i < 6; i++) {
            if (i != _mainPlayerId)
            {
                _renderPlayer[i].transform.position = Vector3.Lerp(_renderPlayer[i].transform.position,
                    _snapshot.playerStates[i].Go.transform.position,
                    visualSmoothCoef);
                _renderPlayer[i].transform.rotation = Quaternion.Slerp(_renderPlayer[i].transform.rotation,
                    _snapshot.playerStates[i].Go.transform.rotation,
                    visualSmoothCoef);
            }
            else {
                _renderPlayer[i].transform.position = Vector3.Lerp(_renderPlayer[i].transform.position,
                    _snapshot.playerStates[i].Go.transform.position,
                    mainPlayerSmoothCoef);
                _renderPlayer[i].transform.rotation = Quaternion.Slerp(_renderPlayer[i].transform.rotation,
                    _snapshot.playerStates[i].Go.transform.rotation,
                    mainPlayerSmoothCoef);
            }
        }
    }

    // Initialize all the cubes on the plane with distance with each other
    // and form a square.
    // @param accepts the prefab of cube as the game object
    // @returns an array that contains all the rigid body objects
    private static void InitSceneCubes(GameObject prefabPhy, GameObject prefabRender, out RBObj[] cubeStates, out Transform[] renderTransforms) {
        float bound = 8.0f;
        float space = 1.6f; // the gap between each cube
        int n = 0;
        for (float i = -bound; i < bound; i += space) n++;
        n *= n;
        cubeStates = new RBObj[n];
        renderTransforms = new Transform[n];
        for (float i = -bound; i < bound; i += space) {
            for (float j = -bound; j < bound; j += space) {
                n--;
                RBObj rBObj = new RBObj {
                    Id = n,
                    Position = new Vector3(i, 3.0f, j),
                    Rotation = Quaternion.identity,
                    LVelocity = Vector3.zero,
                    AVelocity = Vector3.zero,
                    Go = Instantiate(prefabPhy, new Vector3(i, 1.0f, j), Quaternion.identity),
                    Priority = 1
                };

                rBObj.Go.GetComponent<RBObjHolder>().rBObj = rBObj;
                cubeStates[n] = rBObj;


                renderTransforms[n] = Instantiate(prefabRender, new Vector3(i, 1.0f, j), Quaternion.identity).transform;
            }
        }
    }

    // Initialize the players on the plane.
    // @param accepts the prefab of sphere as the game object
    // @returns an array that contains all the rigid body objects
    private static RBObj[] InitPlayers(GameObject prefab, GameObject prefabRender, out GameObject[] renderTransforms) {
        int n = 6;
        var res = new RBObj[n];
        renderTransforms = new GameObject[6];
        for (int i = 0; i < n; i++) {
            Vector3 v3 = new Vector3(0.4f * Mathf.Sin(Mathf.PI * (float) i / (float) (n - 1)), 3, 0.4f * Mathf.Cos(Mathf.PI * (float) i / (float) (n - 1)));
            RBObj rBObj = new RBObj {
                Id = i,
                    Position = v3,
                    Rotation = Quaternion.identity,
                    LVelocity = Vector3.zero,
                    AVelocity = Vector3.zero,
                    Go = Instantiate(prefab, v3, Quaternion.identity),
                    Priority = 1
            }.SetActive(false);

            rBObj.Go.GetComponent<RBObjHolder>().rBObj = rBObj;

            res[i] = rBObj;

            renderTransforms[i] = Instantiate(prefabRender, new Vector3(v3.x, v3.y, v3.z), Quaternion.identity);
            renderTransforms[i].SetActive(false);
        }
        return res;
    }

    public Snapshot Snapshot { get => _snapshot; }
    public void UpdateSnapshot() => _snapshot.UpdateFromRigid();


    // 1. Set Snapshot property
    // 2. Call apply to update physic engine
    public void ApplySnapshot(Snapshot snapshot)
    {
        foreach (RBObj rbObj in snapshot.cubeStates)
        {
            RBObj localVObj = _snapshot.cubeStates[rbObj.Id];
            if (rbObj.Priority > localVObj.Priority) { 
                // Just set the position and orientation directly
                localVObj.ApplyRB(
                    rbObj.Position,
                    rbObj.Rotation,
                    rbObj.LVelocity,
                    rbObj.AVelocity
                );
                if (isServer)
                {
                    rbObj.Priority = localVObj.Priority;
                }
                else {
                    rbObj.Priority = 0;
                }
            }
        }

        foreach (RBObj player in _snapshot.playerStates)
        {
            player.SetActive(true);
            _renderPlayer[player.Id].SetActive(true);
        }

            // Disable "inactive" players
            foreach (RBObj player in _snapshot.playerStates)
        {
            // player.SetActive(false);
        }

        foreach (RBObj player in snapshot.playerStates)
        {
            if (player.Id == _snapshot.playerStates[_mainPlayerId].Id) {
                // Do not let anyone else control this player
                continue;
            }
            RBObj localVObj = _snapshot.playerStates[player.Id];
            // localVObj.SetActive(true);
                // Just set the position and orientation directly
            localVObj.ApplyRB(
                player.Position,
                player.Rotation,
                player.LVelocity,
                player.AVelocity
            );
        }

    }

    public int getMainPlayerID() {
        return _mainPlayerId;
    }
}