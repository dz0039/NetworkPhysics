using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Client : MonoBehaviour {
    /*
         client side:
         send snapshot on frame n_client
         server side:
         receiving all clients
         for each cube
             render use the most recent snapshot
         get snapshot from updated cube
             send
    */
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
    void Update() {
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