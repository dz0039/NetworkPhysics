using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Test : MonoBehaviour {
    void testBitStream() {
        BitStreamWriter writer;
        BitStreamReader reader;

        // Resize 
        writer = new BitStreamWriter(2);
        writer.writeInt32(0);
        Assert.IsTrue(writer.getByteLength()==4);


        // Concatnat bits
        // 1, 0000010 11000011, 1
        // should be 00000101 10000110 00000011
        writer = new BitStreamWriter();
        writer.writeBool(true);
        writer.writeInt16(707);
        writer.writeBool(true);
        var data = writer.getData();
        Assert.IsTrue(data.Length==3 && data[0]==0x05 && data[1]==0x86 && data[2]==0x03);
    }

    void Start() {
        testBitStream();
    }
}