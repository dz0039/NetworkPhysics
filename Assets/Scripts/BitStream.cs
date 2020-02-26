using System;
using UnityEngine;
/**
    Bit Stream Reader and Writer, data are bit-aligned, all Big-endian
**/
public class BitStreamReader {
    private int _head;
    private byte[] _data;

    public BitStreamReader(byte[] data) {
        _head = 0;
        _data = data;
    }

    // int32
    public int readInt32() {
        // int num = 0;
        // for (int i = 0; i < 4; i++) {
        //     num |= readBits(_data[getHeadInBytes()], 8);
        //     _data[getHeadInBytes()]
        //     writeBits((byte)(val >> (3-i)*8), 8);
        //     _head += 8;
        // }
        return 1;
    }

    public bool readBool() {
        return true;
    }

    private byte readBits(byte data, int bitCount) {
        return (byte) 1;
    }

    public int getHeadInBytes() {
        return (_head + 7) >> 3;
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

    public void writeInt32(int val) {
        for (int i = 0; i < 4; i++) {
            byte b = (byte) (val >>(3 - i) * 8);
            writeBits(b, 8);
        }
    }
    public void writeInt16(int val) {
        for (int i = 0; i < 2; i++) {
            byte b = (byte) (val >>(1 - i) * 8);
            writeBits(b, 8);
        }
    }

    private void writeBits(byte data, int bit_count) {
        // TODO: resize _data)

        byte bit2write;
        int curr_byte = _head >> 3;
        int used_this_byte = _head & 7;
        int space_this_byte = 8 - used_this_byte;
        int bit_left_this = Math.Min(space_this_byte, bit_count);
        bit2write = (byte) ((data & ~(0xff << bit_left_this)) << used_this_byte);
        _data[curr_byte] |= bit2write;

        // next byte
        int bit_left_next = bit_count - bit_left_this;
        if (bit_left_next > 0) {
            bit2write = (byte) (data << (8 - bit_count) >>(8 - bit_count + bit_left_this));
            _data[curr_byte + 1] = bit2write;
        }
        _head += bit_count;
    }

    public int getByteLength() {
        return (_head + 7) >> 3;
    }
}