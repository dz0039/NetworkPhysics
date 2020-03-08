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
    private string serverAddr = "127.0.0.1"; // Temp
    private int serverPort = 3939; // Temp
    private UDPSocket socket;
    private Queue<byte[]> serverSnapshots;

    private Snapshot _snapshot;

    void Start() {
        Assert.IsNull(FindObjectOfType<Host>());

        this.serverSnapshots = new Queue<byte[]>();
        socket = new UDPSocket(serverSnapshots);
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
        while (serverSnapshots.Count != 0) {
            byte[] packet = serverSnapshots.Dequeue();

            Snapshot.FromBytes(_snapshot, packet);
            Game.Instance.ApplySnapshot(_snapshot, false);
        }
    }
}