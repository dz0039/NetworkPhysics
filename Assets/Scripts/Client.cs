using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Client : MonoBehaviour {
    const float c_interval = 1f/6f;
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

        _snapshot = Game.Instance.GetSnapshot().Clone();
    }
    // Update is called once per frame
    void FixedUpdate() {
        _timeUntilNextUpdate -= Time.deltaTime;
        if (_timeUntilNextUpdate < 0)
            _timeUntilNextUpdate = c_interval;
        else
            return;


        // Send snapshot to sever
        Snapshot clientSnapshot = Game.Instance.GetSnapshot();
        byte[] asBytes = Snapshot.ToBytes(clientSnapshot);
        socket.ClientSend(asBytes);

        // Then recieve snapshot from server
        while (_serverMsg.Count != 0) {
            byte[] packet = _serverMsg.Dequeue();

            Snapshot.FromBytes(_snapshot, packet);
            Game.Instance.ApplySnapshot(_snapshot, false);
        }
    }
}