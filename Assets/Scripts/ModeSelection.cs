using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ModeSelection : MonoBehaviour {
    bool isStarted = false;
    string addrstr = "host addr";
    string portstr = "host port";
    IPEndPoint localIP;

    Host host = null;
    Client client = null;

    void Start() {
        using(Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint;
        }
    }

    void OnGUI() {
        if (!isStarted) {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200));
            if (GUI.Button(new Rect(10, 10, 180, 30), "As Host")) {
                host = gameObject.AddComponent<Host>();
                host.Init(localIP.Address.ToString(), localIP.Port);
                isStarted = true;
            }
            GUI.Box(new Rect(10, 50, 180, 40), localIP.ToString());

            if (GUI.Button(new Rect(200, 10, 180, 30), "As Client")) {
                client = gameObject.AddComponent<Client>();
                client.Init(addrstr, Convert.ToInt32(portstr));
                isStarted = true;
            }
            addrstr = GUI.TextField(new Rect(200, 50, 130, 40), addrstr);
            portstr = GUI.TextField(new Rect(330, 50, 50, 40), portstr);

            GUI.EndGroup();

        } else {
            if (host) {
                GUI.Box(new Rect(0, 0, 200, 40), "[Host] " + host.HostIP);
            } else {
                GUI.Box(new Rect(0, 0, 200, 40), "[Client] " + client.HostIP);
            }
        }
    }

}