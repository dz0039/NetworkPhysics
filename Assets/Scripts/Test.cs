using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Test : MonoBehaviour {
    void testBitStream() {
        BitStreamWriter writer;
        BitStreamReader reader;

        writer = new BitStreamWriter();
        writer.writeBool(true);
        writer.writeInt16(707);
        writer.writeBool(true);
        // 1, 0000010 11000011, 1
        // should be 00000101 10000110 00000011
        var data = writer.getData();
        Assert.IsTrue(data[0]==0x05 && data[1]==0x86 && data[2]==0x03);
    }

    void Start() {
        testBitStream();
    }
}