using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

public class Test : MonoBehaviour {
#region  Helper
    static bool IsApprox(Quaternion q1, Quaternion q2) {
        // 1 deg in 1 axis -> 0.0000004f
        return Mathf.Abs(Quaternion.Dot(q1, q2)) >= 1 - 0.0000004f;
    }

#endregion


    static void TestBitStream() {
        BitStreamWriter writer = new BitStreamWriter(2);
        BitStreamReader reader = new BitStreamReader();

        // Resize, basic int
        writer.WriteInt32(0x12345678);
        Assert.IsTrue(writer.LengthInBytes == 4);
        reader = new BitStreamReader(writer.DumpBytes());
        Assert.IsTrue(reader.ReadInt32() == 0x12345678);

        // Concatnat bits
        // 1, 0000010 11000011, 1
        // should be 00000101 10000110 00000011
        writer.WriteBool(true);
        writer.WriteInt16(707);
        writer.WriteBool(true);
        var data = writer.DumpBytes();
        Assert.IsTrue(data.Length == 3);
        Assert.IsTrue(data[0] == 0x05);
        Assert.IsTrue(data[1] == 0x86);
        Assert.IsTrue(data[2] == 0x03);
        reader.SetBytes(data);
        Assert.IsTrue(reader.ReadBool());
        Assert.IsTrue(reader.ReadInt16() == 707);
        Assert.IsTrue(reader.ReadBool());

        // float
        writer.WriteBool(false);
        writer.WriteFloat(12345.012345f);
        writer.WriteBool(true);
        reader.SetBytes(writer.DumpBytes());
        Assert.IsFalse(reader.ReadBool());
        Assert.IsTrue(reader.ReadFloat() == 12345.012345f);
        Assert.IsTrue(reader.ReadBool());

        // vector3, quaternion
        Vector3 vec = new Vector3(-0.51231f, 0.113123f, 1.1231123f);
        Quaternion q = new Quaternion(-0.123f, 0.345f, 0.678f, -0.23f);
        q.Normalize();
        writer.WriteQuaternionRot(q);
        writer.WriteBool(false);
        writer.WriteVector3(Vector3.one);
        writer.WriteVector3(vec);
        reader.SetBytes(writer.DumpBytes());
        Assert.IsTrue(IsApprox(reader.ReadQuaternionRot(), q));
        Assert.IsFalse(reader.ReadBool());
        Assert.IsTrue(reader.ReadVector3() == Vector3.one);
        Assert.IsTrue(reader.ReadVector3() == vec);
    }

    // Test the UDP code for both server and client
    static void TestUDP() {
        string sAddr = "127.0.0.1";
        int sPort = 9019;
        int cCount = 2;

        BitStreamWriter writer = new BitStreamWriter();
        BitStreamReader reader = new BitStreamReader();

        // 1 server, cCount client.
        // per client send msg twices
        Dictionary<EndPoint, Queue<byte[]>> ep2msg = new Dictionary<EndPoint, Queue<byte[]>>();
        UDPSocket sv = new UDPSocket(ep2msg);
        sv.Server(sAddr,sPort);
        Thread[] threads = new Thread[cCount];
        EndPoint[] eps = new EndPoint[cCount];
        List<byte[]> msgs = new List<byte[]>();
        for (int i = 0; i < cCount; i++) {
            msgs.Add(writer.WriteInt16(i).DumpBytes());
            int ti = i;
            threads[i] = new Thread(() => {
                var client = new UDPSocket(new Queue<byte[]>());
                client.Client(sAddr, sPort);
                client.cSend(msgs[ti]);
                client.cSend(msgs[ti]);
                eps[ti] = client.Socket.LocalEndPoint;
            });
            threads[i].Start();
        }
        foreach (var t in threads) {
            t.Join();
        }
        SpinWait.SpinUntil(()=>{
            return ep2msg.Keys.Count == cCount;
        }, 5000);

        Assert.IsTrue(ep2msg.Keys.Count == cCount);
        for (int i = 0; i < cCount; i++) {
            int num1 = reader.SetBytes(ep2msg[eps[i]].Dequeue()).ReadInt16();
            int num2 = reader.SetBytes(ep2msg[eps[i]].Dequeue()).ReadInt16();
            Assert.IsTrue(num1 == i);
            Assert.IsTrue(num2 == i);
        }
    }

    void Start() {
        Debug.Log("TestBitstream----");
        TestBitStream();
        Debug.Log("TestUDP----");
        TestUDP();

        
        Debug.Log("---All Test Finished--");
    }
}