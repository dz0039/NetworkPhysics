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
public class Game : MonoBehaviour
{
    public GameObject cubePrefab;
    private RBObj[] _sceneCubes; // all scene cubes
    public GameObject playerPrefab;
    private RBObj[] _playerObjs;
    private int _mainPlayerId; // should always be 0

    private Snapshot _snapshot;

    private static Game _instance;
    public static Game Instance { get => _instance; }

    void Start()
    {
        // instance should be null at first
        if (_instance && _instance != this)
        {
            Debug.LogError("Singleton Error");
            Destroy(gameObject);
            return;
        }
        Assert.IsNull(_instance);
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // init scene
        _sceneCubes = InitSceneCubes(cubePrefab);
        Debug.Log("[Init cubes]" + _sceneCubes.Length);
        _playerObjs = InitPlayers(playerPrefab);
        Debug.Log("[Init players]" + _playerObjs.Length);
        // init main player
        Assert.IsTrue(_playerObjs.Length > 1);
        _mainPlayerId = 0;
        _playerObjs[_mainPlayerId].Go.AddComponent<PlayerController>();
        _playerObjs[_mainPlayerId].SetRB(
            new Vector3(0, 1, 0),
            Quaternion.identity,
            Vector3.zero,
            Vector3.zero
        ).SetActive(true);
    }

    void Update()
    {
        // hermit interpolation using current snapshot

    }

    // Initialize all the cubes on the plane with distance with each other
    // and form a square.
    // @param accepts the prefab of cube as the game object
    // @returns an array that contains all the rigid body objects
    private static RBObj[] InitSceneCubes(GameObject prefab)
    {
        float bound = 8.0f;
        float space = 1.6f; // the gap between each cube
        int n = 0;
        for (float i = -bound; i < bound; i += space) n++;
        n *= n;
        var res = new RBObj[n];
        for (float i = -bound; i < bound; i += space)
        {
            for (float j = -bound; j < bound; j += space)
            {
                res[--n] = new RBObj
                {
                    Id = n,
                    Position = new Vector3(i, 3.0f, j),
                    Rotation = Quaternion.identity,
                    LVelocity = Vector3.zero,
                    AVelocity = Vector3.zero,
                    Go = Instantiate(prefab, new Vector3(i, 1.0f, j), Quaternion.identity)
                };
            }
        }
        return res;
    }

    // Initialize the players on the plane.
    // @param accepts the prefab of sphere as the game object
    // @returns an array that contains all the rigid body objects
    private static RBObj[] InitPlayers(GameObject prefab)
    {
        int n = 6;
        var res = new RBObj[n];
        for (int i = 0; i < n; i++)
        {
            res[i] = new RBObj
            {
                Id = i,
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                LVelocity = Vector3.zero,
                AVelocity = Vector3.zero,
                Go = Instantiate(prefab, Vector3.zero, Quaternion.identity)
            }.SetActive(false);
        }
        return res;
    }

    // Gets the snapshot of the current game scene
    // @returns a Snapshot object
    public Snapshot GetSnapshot()
    {
        return null;
    }

    public void AddSnapshot(Snapshot snapshot)
    {
        // TODO:
        // for each cube?
        //   update status using one with highest priority if in (mySnap U newSnap)
        //   recalculating priority
        // adjust mySnap
        // for each player in mySnap
        //   not exist in scene -> create
        //   not found -> diable
    }

}

public class RBObj
{
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 LVelocity { get; set; }  // TODO: what velocity?
    public Vector3 AVelocity { get; set; }  // angular velocity
    public GameObject Go { get; set; }
    public int Priority { get; set; } // only associate with main player

    private Rigidbody _rb = null;

    public RBObj SetActive(bool val)
    {
        // TODO: RB errors?
        Go.SetActive(val);
        return this;
    }

    public RBObj SetRB(Vector3 pos, Quaternion rot, Vector3 lv, Vector3 av)
    {
        if (!_rb) _rb = Go.GetComponent<Rigidbody>();
        // TODO: hermit
        _rb.position = pos;
        _rb.rotation = rot;
        _rb.velocity = lv;
        _rb.angularVelocity = av;
        return this;
    }
}

public class Snapshot
{
    public List<RBObj> cubeStates;
    public List<RBObj> playerStates;
}