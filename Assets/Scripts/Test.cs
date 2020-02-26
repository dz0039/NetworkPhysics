using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Test : MonoBehaviour {
    static bool isApprox(Quaternion q1, Quaternion q2) {
        // 1 deg in 1 axis -> 0.0000004f
        return Mathf.Abs(Quaternion.Dot(q1, q2)) >= 1 - 0.0000004f;
    }

    void testBitStream() {
        BitStreamWriter writer;
        BitStreamReader reader;

        // Resize, basic int
        writer = new BitStreamWriter(2);
        writer.writeInt32(0x12345678);
        Assert.IsTrue(writer.getByteLength() == 4);
        reader = new BitStreamReader(writer.getData());
        Assert.IsTrue(reader.readInt32() == 0x12345678);

        // Concatnat bits
        // 1, 0000010 11000011, 1
        // should be 00000101 10000110 00000011
        writer = new BitStreamWriter();
        writer.writeBool(true);
        writer.writeInt16(707);
        writer.writeBool(true);
        var data = writer.getData();
        Assert.IsTrue(data.Length == 3 && data[0] == 0x05 && data[1] == 0x86 && data[2] == 0x03);
        reader = new BitStreamReader(data);
        Assert.IsTrue(reader.readBool());
        Assert.IsTrue(reader.readInt16() == 707);
        Assert.IsTrue(reader.readBool());

        // float
        writer = new BitStreamWriter();
        writer.writeBool(false);
        writer.writeFloat(12345.012345f);
        writer.writeBool(true);
        reader = new BitStreamReader(writer.getData());
        Assert.IsFalse(reader.readBool());
        Assert.IsTrue(reader.readFloat() == 12345.012345f);
        Assert.IsTrue(reader.readBool());

        // vector3, quaternion
        writer = new BitStreamWriter(2);
        Vector3 vec = new Vector3(-0.51231f, 0.113123f, 1.1231123f);
        Quaternion q = new Quaternion(-0.123f, 0.345f, 0.678f, -0.23f);
        q.Normalize();
        writer.writeQuaternionRot(q);
        writer.writeBool(false);
        writer.writeVector3(Vector3.one);
        writer.writeVector3(vec);
        reader = new BitStreamReader(writer.getData());
        Assert.IsTrue(isApprox(reader.readQuaternionRot(),q));
        Assert.IsFalse(reader.readBool());
        Assert.IsTrue(reader.readVector3() == Vector3.one);
        Assert.IsTrue(reader.readVector3() == vec);

        // quarternion

        Debug.Log("---Finish TestBitstream");
    }

    void Start() {
        testBitStream();
    }
}