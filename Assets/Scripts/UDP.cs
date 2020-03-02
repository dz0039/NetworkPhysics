using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

/**
    Wrapper on UDPClient, multi-thread
**/
// public class UDP : MonoBehaviour
// {
//     void Start()
//     {
//         Dictionary<EndPoint, Queue<string>> messageDictionary = new Dictionary<EndPoint, Queue<string>>();
//         using (UDPSocket s = new UDPSocket(messageDictionary))
//         {
//             s.Server("all", 37373);
//             Debug.Log("hehe");
//             while (true)
//             {
//                 Debug.Log("hehe");
//                 //Servery Stuff Goes Here.
//                 //Like reiteratively dequeuing the Message Dictionary Queues and processing/replying to all commands/etc...
//             }
//         }
//     }

//     void Update()
//     {

//     }
// }
public class UDPSocket : IDisposable
{
    //Constant for configuring the prevention of ICMP connection resets
    private const int SIO_UDP_CONNRESET = -1744830452;

    //UDP socket
    private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    //Buffer Size Constant
    private const int bufSize = 8 * 1024;

    //Raw string data from client packets
    private Dictionary<EndPoint, Queue<string>> messageDictionary;

    //Queue for holding raw string data from server packets when in client mode.
    private Queue<string> cQ;

    //Boolean to determine which mode we're in so received messages get put in the right place.
    private bool clientMode = false;

    //Max string length allowed by the servers.
    private int maxMessageLength = 1450;

    //IDisposable stuff
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // free managed resources
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
        }
    }

    //State class for async receive.
    public class State
    {
        public byte[] buffer = new byte[bufSize];
        public EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    }

    //Server "Mode" 
    public UDPSocket(Dictionary<EndPoint, Queue<string>> msgsDict)
    {
        clientMode = false;
        messageDictionary = msgsDict;
    }

    //Client "Mode"
    public UDPSocket(Queue<string> mq)
    {
        clientMode = true;
        cQ = mq;
    }

    public void CloseSocket()
    {
        _socket.Close();
    }

    //Start/Bind a Server.
    public void Server(string address, int port)
    {
        // //In case restarting uncleanly, dunno if this actually does anything..  
        // _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        // //Ensure all async packets contain endpoint info and etc.
        // _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        // //Ignore ICMP port unreachable exceptions.
        // _socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        //Bind to port.
        if (address == "all")
        {
            Debug.Log("hehe for all");
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }
        else
        {
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        }
        //Start receive callback process.
        Receive();
    }

    //Setup a Client to Server socket.
    public void Client(string address, int port)
    {
        //Dunno if these two options do anything for client sockets, but they don't seem to break anything.
        // _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        // _socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        _socket.Connect(IPAddress.Parse(address), port);
        //Start receive callback.
        Receive();
    }

    //ServerSend sends to any EndPoint from THIS server.
    public void ServerSend(string text, EndPoint ep)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(text);

            _socket.SendTo(data, ep);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ServerSend Exception: " + ex.Message);
        }
    }

    //Client Send only sends to the connected Server.
    public void cSend(string text)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            _socket.Send(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine("cSend Exception: " + ex.Message);
        }
    }

    //Setup Async Callback
    private void Receive()
    {
        try
        {
            State so = new State();
            _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref so.epFrom, new AsyncCallback(_Receive), so);
        }
        catch (Exception)
        {
        }
    }

    //Receive Callback
    private void _Receive(IAsyncResult ar)
    {
        try
        {
            // store the state through the async operation
            State so = (State)ar.AsyncState;

        
            int bytes = _socket.EndReceiveFrom(ar, ref so.epFrom);
            string smessage = Encoding.ASCII.GetString(so.buffer, 0, bytes);
            //Console.WriteLine("FROM NET: " + text);
            if (smessage.Length < maxMessageLength)
            {
                if (clientMode)
                {
                    cQ.Enqueue(smessage);
                }
                else
                {
                    if (!messageDictionary.ContainsKey(so.epFrom))
                    {
                        Debug.Log("enter containskey for receive");
                        messageDictionary.Add(so.epFrom, new Queue<string> { });
                    }
                    messageDictionary[so.epFrom].Enqueue(smessage);
                    Debug.Log("Inner dic size is: " + messageDictionary.Count);
                }
            }
            _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref so.epFrom, new AsyncCallback(_Receive), so);
        }
        catch (Exception)
        {
        }
    }

    // rep exposure, but we do need to get the things in server's dictionary
    public Dictionary<EndPoint, Queue<string>> getServerDictionary() {
        return messageDictionary;
    }
}