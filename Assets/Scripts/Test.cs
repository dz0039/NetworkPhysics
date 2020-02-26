using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Test : MonoBehaviour {
    void testBitStream() {
        BitStreamWriter writer;
        BitStreamReader reader;
        int num1, num2;

        // Resize, basic integer
        writer = new BitStreamWriter(2);
        writer.writeInt32(0x12345678);
        Assert.IsTrue(writer.getByteLength()==4);
        reader = new BitStreamReader(writer.getData());
        num1 = reader.readInt32();
        Assert.IsTrue(num1 == 0x12345678);

        // Concatnat bits
        // 1, 0000010 11000011, 1
        // should be 00000101 10000110 00000011
        writer = new BitStreamWriter();
        writer.writeBool(true);
        writer.writeInt16(707);
        writer.writeBool(true);
        var data = writer.getData();
        Assert.IsTrue(data.Length==3 && data[0]==0x05 && data[1]==0x86 && data[2]==0x03);
        reader = new BitStreamReader(data);
        Assert.IsTrue(reader.readBool());
        Assert.IsTrue(reader.readInt16() == 707);
        Assert.IsTrue(reader.readBool());

        Debug.Log("---Finish TestBitstream");
    }

    void Start() {
        testBitStream();
    }
}