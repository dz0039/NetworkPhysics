using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Client : MonoBehaviour {
    const float c_interval = 1f/10f;
    float _timeUntilNextUpdate = 0.0f;

    public string HostIP { get => socket.Socket.RemoteEndPoint.ToString(); }
    private string serverAddr;
    private int serverPort;
    private UDPSocket socket;
    private Queue<byte[]> _serverMsg;

    private Snapshot _snapshot;

    void Start() {
        Assert.IsNull(FindObjectOfType<Host>());
    }

    public void Init(string address, int port) {
        serverAddr = address;
        serverPort = port;

        this._serverMsg = new Queue<byte[]>();
        socket = new UDPSocket(_serverMsg);
        socket.Client(serverAddr, serverPort);

        _snapshot = Game.Instance.Snapshot.Clone();
    }

    // Close the client connection with the server. 
    public void Close() {
        socket.CloseSocket();
    }

    // Update is called once per frame
    void FixedUpdate() {
        _timeUntilNextUpdate -= Time.fixedDeltaTime;
        if (_timeUntilNextUpdate < 0)
            _timeUntilNextUpdate = c_interval;
        else
            return;

        Game.Instance.UpdateSnapshot();

        // Then send modified snapshot back to server
        List<RBObj> priorityPlayers = new List<RBObj>();
        List<RBObj> priorityCubes = new List<RBObj>(); //  Game.Instance.Snapshot.getPriorityCubes(50);

        priorityPlayers.Add(Game.Instance.Snapshot.playerStates[Game.Instance.getMainPlayerID()]);

        // Send snapshot to sever
        Snapshot clientSnapshot = new Snapshot(priorityPlayers, priorityCubes); // Game.Instance.Snapshot;
        byte[] asBytes = Snapshot.ToBytes(clientSnapshot);
        socket.ClientSend(asBytes);

        // Now clear the priority value of the cubes we just sent
        Game.Instance.Snapshot.clearPriority(priorityCubes);

        // Recieve snapshot from server
        while (_serverMsg.Count != 0)
        {
            byte[] packet = _serverMsg.Dequeue();
            Snapshot recieved = Snapshot.FromBytes(packet);
            Game.Instance.ApplySnapshot(recieved);
        }
    }
}