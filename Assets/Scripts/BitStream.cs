using System;
using UnityEngine;
using UnityEngine.Assertions;

/**
    Bit Stream Reader and Writer, data are bit-aligned, all Big-endian
**/
// TODO: compress int, quaternion
// TODO: remove tmp resources
public class BitStreamReader {
    private int _head;
    private byte[] _data;


    public BitStreamReader() {
        _head = 0;
        _data = null;
    }

    public BitStreamReader(byte[] data) {
        _head = 0;
        _data = data;
    }


    // // append bytes to the end of current BitReader
    // public BitStreamReader AddBytes(byte[] data) {
    //     _head = 0;
        

    //     return this;
    // }

    public BitStreamReader SetBytes(byte[] data) {
        _head = 0;
        _data = data;
        return this;
    }

    public bool ReadBool() {
        return (ReadBits(1) & 1) == 0 ? false : true;
    }

    public int ReadInt16() {
        int num = 0;
        for (int i = 0; i < 2; i++) {
            num |= ReadBits(8) << (1 - i) * 8;
        }
        return num;
    }

    public int ReadInt32() {
        int num = 0;
        for (int i = 0; i < 4; i++) {
            num |= ReadBits(8) << (3 - i) * 8;
        }
        return num;
    }

    public float ReadFloat() {
        byte[] data = new byte[4];
        for (int i = 0; i < 4; i++)
            data[i] = ReadBits(8);
        return (BitConverter.ToSingle(data, 0));
    }

    public Vector3 ReadVector3() {
        return (new Vector3(ReadFloat(), ReadFloat(), ReadFloat()));
    }

    public Quaternion ReadQuaternionRot() {
        Quaternion q = new Quaternion();
        q.x = ReadFloat();
        q.y = ReadFloat();
        q.z = ReadFloat();
        
        float temp = 1f - q.x*q.x - q.y*q.y -q.z*q.z;
        if (temp > 0) {
            q.w = (float) Math.Sqrt(1f - q.x*q.x - q.y*q.y -q.z*q.z);
        } else {
            q.w = 0;
        }
        return q;
    }

    private byte ReadBits(int bitCount) {
        Assert.IsTrue(bitCount <= 8 && bitCount > 0);
        Assert.IsTrue((_head + bitCount + 7) >> 3 <= _data.Length);

        byte b = 0x00;

        // this byte
        int i_this_byte = _head >> 3;
        int readed_this_byte = _head & 7;
        int left_this_byte = Math.Min(8 - readed_this_byte, bitCount);
        int space_this_byte = 8 - left_this_byte - readed_this_byte;
        // 0000(011)[1], readed = 1, left = 3, space=4
        b |= (byte) (_data[i_this_byte] << space_this_byte >>(space_this_byte + readed_this_byte));

        // next byte
        int left_next_byte = bitCount - left_this_byte;
        if (left_next_byte > 0) {
            b |= (byte) ((_data[i_this_byte + 1] & ~(0xff << left_next_byte)) << left_this_byte);
        }

        _head += bitCount;
        return b;
    }
}

public class BitStreamWriter {
    private int _capacity;
    private int _head;
    private byte[] _data;

    public int LengthInBytes {
        get => (_head+7)>>3;
    }

    public BitStreamWriter(int capacity = 512 * 8) {
        _capacity = capacity;
        _head = 0;
        _data = new byte[capacity];
    }

    public byte[] DumpBytes() {
        int len = LengthInBytes;
        byte[] b = new byte[len];
        Buffer.BlockCopy(_data, 0, b, 0, len);
        // clean last byte
        // int validBitsInLastByte = _head & 7;
        // if (validBitsInLastByte != 0) {
        //     b[len-1] = (byte) (b[len-1] & (~(0xff << validBitsInLastByte)));
        // }

        _head = 0;
        Array.Clear(_data, 0, len);
        return b;
    }

    public BitStreamWriter WriteBool(bool val) {
        WriteBits(val ? (byte) 1 : (byte) 0, 1);
        return this;
    }

    public BitStreamWriter WriteInt16(int val) {
        Assert.IsTrue((val & 0xffff) == val);

        for (int i = 0; i < 2; i++) {
            byte b = (byte) (val >>(1 - i) * 8);
            WriteBits(b, 8);
        }
        return this;
    }

    public BitStreamWriter WriteInt32(int val) {
        for (int i = 0; i < 4; i++) {
            byte b = (byte) (val >>(3 - i) * 8);
            WriteBits(b, 8);
        }
        return this;
    }

    public BitStreamWriter WriteFloat(float val) {
        byte[] data = BitConverter.GetBytes(val);
        foreach (var b in data)
            WriteBits(b, 8);
        return this;
    }

    public BitStreamWriter WriteVector3(Vector3 vec3) {
        WriteFloat(vec3.x);
        WriteFloat(vec3.y);
        WriteFloat(vec3.z);
        return this;
    }

    public BitStreamWriter WriteQuaternionRot(Quaternion q) {
        // TODO: smallest-3. https://gafferongames.com/post/snapshot_compression/
        Assert.IsTrue(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z - 1.0f < 0.0001f);
        if (q.w < 0) {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
        }
        WriteFloat(q.x);
        WriteFloat(q.y);
        WriteFloat(q.z);
        return this;
    }

    private void WriteBits(byte data, int bitCount) {
        /**
            e.g, from: 1, 0000010 11000011, 1
            to: 00000101 10000110 00000011
        **/
        Assert.IsTrue(bitCount <= 8 && bitCount > 0);

        while (_head + bitCount > _capacity) {
            Array.Resize<byte>(ref _data, _capacity * 2);
            _capacity *= 2;
        }
        int i_this_byte = _head >> 3;
        int used_this_byte = _head & 7;
        int bit_left_this = Math.Min(8 - used_this_byte, bitCount);
        byte bit2write = (byte) ((data & ~(0xff << bit_left_this)) << used_this_byte);

        int bit_total_this = used_this_byte + bit_left_this;
        // _data[i_this_byte] = (byte)((_data[i_this_byte] | bit2write) & (~(0xff << bit_total_this)));
        _data[i_this_byte] |= bit2write;

        // next byte
        int bit_left_next = bitCount - bit_left_this;
        if (bit_left_next > 0) {
            bit2write = (byte) (data << (8 - bitCount) >>(8 - bitCount + bit_left_this));
            _data[i_this_byte + 1] = bit2write;
        }

        _head += bitCount;
    }

}