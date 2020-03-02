﻿using UnityEngine;
using UnityEngine.Assertions;

using System.Collections.Generic;

using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;

public class Test : MonoBehaviour
{
    static bool isApprox(Quaternion q1, Quaternion q2)
    {
        // 1 deg in 1 axis -> 0.0000004f
        return Mathf.Abs(Quaternion.Dot(q1, q2)) >= 1 - 0.0000004f;
    }

    static void TestBitStream()
    {
        BitStreamWriter writer;
        BitStreamReader reader;

        // Resize, basic int
        writer = new BitStreamWriter(2);
        writer.WriteInt32(0x12345678);
        Assert.IsTrue(writer.LengthInBytes == 4);
        reader = new BitStreamReader(writer.GetBytes());
        Assert.IsTrue(reader.ReadInt32() == 0x12345678);

        // Concatnat bits
        // 1, 0000010 11000011, 1
        // should be 00000101 10000110 00000011
        writer = new BitStreamWriter();
        writer.WriteBool(true);
        writer.WriteInt16(707);
        writer.WriteBool(true);
        var data = writer.GetBytes();
        Assert.IsTrue(data.Length == 3 && data[0] == 0x05 && data[1] == 0x86 && data[2] == 0x03);
        reader = new BitStreamReader(data);
        Assert.IsTrue(reader.ReadBool());
        Assert.IsTrue(reader.ReadInt16() == 707);
        Assert.IsTrue(reader.ReadBool());

        // float
        writer = new BitStreamWriter();
        writer.WriteBool(false);
        writer.WriteFloat(12345.012345f);
        writer.WriteBool(true);
        reader = new BitStreamReader(writer.GetBytes());
        Assert.IsFalse(reader.ReadBool());
        Assert.IsTrue(reader.ReadFloat() == 12345.012345f);
        Assert.IsTrue(reader.ReadBool());

        // vector3, quaternion
        writer = new BitStreamWriter(2);
        Vector3 vec = new Vector3(-0.51231f, 0.113123f, 1.1231123f);
        Quaternion q = new Quaternion(-0.123f, 0.345f, 0.678f, -0.23f);
        q.Normalize();
        writer.WriteQuaternionRot(q);
        writer.WriteBool(false);
        writer.WriteVector3(Vector3.one);
        writer.WriteVector3(vec);
        reader = new BitStreamReader(writer.GetBytes());
        Assert.IsTrue(isApprox(reader.ReadQuaternionRot(), q));
        Assert.IsFalse(reader.ReadBool());
        Assert.IsTrue(reader.ReadVector3() == Vector3.one);
        Assert.IsTrue(reader.ReadVector3() == vec);

        Debug.Log("---Finish TestBitstream");
    }

    // Test the UDP code for both server and client
    // it doesn't have assertions but it will test
    // if the server can receive msgs from 2 clients at
    // the same time.
    static void TestUDP()
    {
        Dictionary<EndPoint, Queue<string>> messageDictionary = new Dictionary<EndPoint, Queue<string>>();

        // create a server
        UDPSocket s = new UDPSocket(messageDictionary);
        s.Server("127.0.0.1", 37373);

        // Dictionary<EndPoint, Queue<string>> updatedDictionary = s.getServerDictionary();

        StartMultipleClient();
    }

    // This method creates multiple clients that 
    // uses multi-thread to send msg to server.
    static void StartMultipleClient()
    {
        Thread t1 = new Thread(() =>
        {
            int numberOfSeconds = 0;
            while (numberOfSeconds < 5)
            {
                // Thread.Sleep(1000);

                numberOfSeconds++;
                // create a client and send to the server
                UDPSocket client = new UDPSocket(new Queue<string>());
                client.Client("127.0.0.1", 37373);
                client.cSend("Client222222 send!");

            }
            Debug.Log("I ran for 5 seconds");
        });

        Thread t2 = new Thread(() =>
        {
            int numberOfSeconds = 0;
            while (numberOfSeconds < 4)
            {
                // Thread.Sleep(1000);

                numberOfSeconds++;
                // create a client and send to the server
                UDPSocket client = new UDPSocket(new Queue<string>());
                client.Client("127.0.0.1", 37373);
                client.cSend("Client111111 send!");
            }
            Debug.Log("I ran for 4 seconds");

        });

        t1.Start();
        t2.Start();

        //wait for t1 to finish
        t1.Join();
        //wait for t2 to finish
        t2.Join();
    }

    void Start()
    {
        TestBitStream();
        TestUDP();
    }
}