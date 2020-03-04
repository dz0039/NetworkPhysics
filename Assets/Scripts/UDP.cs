using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPSocket : IDisposable {
    private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    public Socket Socket {
        get => _socket;
    }
    //Raw string data from client packets
    private Dictionary<EndPoint, Queue<byte[]>> _clientMessageDictionary;

    //Queue for holding raw string data from server packets when in client mode.
    private Queue<byte[]> _serverMessageQueue;

    //Boolean to determine which mode we're in so received messages get put in the right place.
    private bool _isClient = false;

    private const int BUF_SIZE = 8 * 1024;

    #region IDisposable
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            // free managed resources
            if (_socket != null) {
                _socket.Dispose();
                _socket = null;
            }
        }
    }

    ~UDPSocket() {
        Dispose(false);
    }
    #endregion
    
    // State class for async receive.
    public class State {
        public byte[] buffer = new byte[BUF_SIZE];
        public EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    }

    // Server "Mode" 
    public UDPSocket(Dictionary<EndPoint, Queue<byte[]>> msgsDict) {
        _isClient = false;
        _clientMessageDictionary = msgsDict;
    }

    // Client "Mode"
    public UDPSocket(Queue<byte[]> mq) {
        _isClient = true;
        _serverMessageQueue = mq;
    }

    public void CloseSocket() {
        _socket.Close();
    }

    // Start/Bind a Server.
    public void Server(string address, int port) {
        //Bind to port.
        if (address == "all") {
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
        } else {
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        }
        //Start receive callback process.
        Receive();
    }

    // Setup a Client to Server socket.
    public void Client(string address, int port) {
        _socket.Connect(IPAddress.Parse(address), port);
        //Start receive callback.
        Receive();
    }

    // ServerSend sends to any EndPoint from THIS server.
    public void ServerSend(byte[] data, EndPoint ep) {
        try {
            _socket.SendTo(data, ep);
        } catch (Exception ex) {
            Console.WriteLine("ServerSend Exception: " + ex.Message);
        }
    }

    // Client Send only sends to the connected Server.
    public void cSend(byte[] data) {
        try {
            _socket.Send(data);
        } catch (Exception ex) {
            Console.WriteLine("cSend Exception: " + ex.Message);
        }
    }

    // Setup Async Callback
    private void Receive() {
        try {
            State so = new State();
            _socket.BeginReceiveFrom(so.buffer, 0, BUF_SIZE, SocketFlags.None, ref so.epFrom, new AsyncCallback(_Receive), so);
        } catch (Exception) { }
    }

    // Receive Callback
    private void _Receive(IAsyncResult ar) {
        try {
            // store the state through the async operation
            State so = (State) ar.AsyncState;

            int dataCount = _socket.EndReceiveFrom(ar, ref so.epFrom);
            byte[] data = new byte[dataCount];
            Array.Copy(so.buffer, data, dataCount);
            if (_isClient) {
                _serverMessageQueue.Enqueue(data);
            } else {
                if (!_clientMessageDictionary.ContainsKey(so.epFrom)) {
                    _clientMessageDictionary.Add(so.epFrom, new Queue<byte[]>() { });
                }
                // Debug message to display the msg from client.
                _clientMessageDictionary[so.epFrom].Enqueue(data);
            }
            _socket.BeginReceiveFrom(so.buffer, 0, BUF_SIZE, SocketFlags.None, ref so.epFrom, new AsyncCallback(_Receive), so);
        } catch (Exception) { }
    }
}