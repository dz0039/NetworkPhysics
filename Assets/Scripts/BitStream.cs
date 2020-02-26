using System;
using UnityEngine;
using UnityEngine.Assertions;
/**
    Bit Stream Reader and Writer, data are bit-aligned, all Big-endian
**/

// TODO: compress int, quaternion

public class BitStreamReader {
    private int _head;
    private byte[] _data;

    public BitStreamReader(byte[] data) {
        _head = 0;
        _data = data;
    }

    public bool readBool() {
        return (readBits(1) & 1) == 0 ? false : true;
    }

    public int readInt16() {
        int num = 0;
        for (int i = 0; i < 2; i++) {
            num |= readBits(8) << (1 - i) * 8;
        }
        return num;
    }

    public int readInt32() {
        int num = 0;
        for (int i = 0; i < 4; i++) {
            num |= readBits(8) << (3 - i) * 8;
        }
        return num;
    }

    public float readFloat() {
        byte[] data = new byte[4];
        for (int i = 0; i < 4; i++)
            data[i] = readBits(8);
        return (BitConverter.ToSingle(data, 0));
    }

    public Vector3 readVector3() {
        return (new Vector3(readFloat(), readFloat(), readFloat()));
    }

    public Quaternion readQuaternionRot() {
        Quaternion q = new Quaternion();
        q.x = readFloat();
        q.y = readFloat();
        q.z = readFloat();
        q.w = (float) Math.Sqrt(1f - q.x*q.x - q.y*q.y -q.z*q.z);
        return q;
    }

    private byte readBits(int bitCount) {
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

    public BitStreamWriter(int capacity = 512 * 8) {
        _capacity = capacity;
        _head = 0;
        _data = new byte[capacity];
    }

    public byte[] getData() {
        int len = getByteLength();
        byte[] b = new byte[len];
        Buffer.BlockCopy(_data, 0, b, 0, len);
        return b;
    }

    public void writeBool(bool val) {
        writeBits(val ? (byte) 1 : (byte) 0, 1);
    }

    public void writeInt16(int val) {
        Assert.IsTrue((val & 0xffff) == val);

        for (int i = 0; i < 2; i++) {
            byte b = (byte) (val >>(1 - i) * 8);
            writeBits(b, 8);
        }
    }

    public void writeInt32(int val) {
        for (int i = 0; i < 4; i++) {
            byte b = (byte) (val >>(3 - i) * 8);
            writeBits(b, 8);
        }
    }

    public void writeFloat(float val) {
        byte[] data = BitConverter.GetBytes(val);
        foreach (var b in data)
            writeBits(b, 8);
    }

    public void writeVector3(Vector3 vec3) {
        writeFloat(vec3.x);
        writeFloat(vec3.y);
        writeFloat(vec3.z);
    }

    public void writeQuaternionRot(Quaternion q) {
        // TODO: smallest-3. https://gafferongames.com/post/snapshot_compression/
        Assert.IsTrue(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z - 1.0f < 0.0001f);
        if (q.w < 0) {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
        }
        writeFloat(q.x);
        writeFloat(q.y);
        writeFloat(q.z);
    }

    private void writeBits(byte data, int bitCount) {
        /**
            e.g, from: 1, 0000010 11000011, 1
            to: 00000101 10000110 00000011
        **/
        Assert.IsTrue(bitCount <= 8 && bitCount > 0);

        if (_head + bitCount > _capacity) {
            Array.Resize<byte>(ref _data, _capacity * 2);
            _capacity *= 2;
        }
        int i_this_byte = _head >> 3;
        int used_this_byte = _head & 7;
        int bit_left_this = Math.Min(8 - used_this_byte, bitCount);
        byte bit2write = (byte) ((data & ~(0xff << bit_left_this)) << used_this_byte);
        _data[i_this_byte] |= bit2write;

        // next byte
        int bit_left_next = bitCount - bit_left_this;
        if (bit_left_next > 0) {
            bit2write = (byte) (data << (8 - bitCount) >>(8 - bitCount + bit_left_this));
            _data[i_this_byte + 1] = bit2write;
        }

        _head += bitCount;
    }

    public int getByteLength() {
        return (_head + 7) >> 3;
    }
}