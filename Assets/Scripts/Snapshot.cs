using System;
using System.Collections.Generic;
using UnityEngine;

/*
snapshot utils and methods
*/

public class RBObj {
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 LVelocity { get; set; } // linear velocity
    public Vector3 AVelocity { get; set; } // angular velocity
    public GameObject Go { get; set; }
    public int Priority { get; set; } // only associate with main player

    private Rigidbody _rb = null;

    public RBObj SetActive(bool val) {
        // TODO: RB errors?
        Go.SetActive(val);
        return this;
    }

    public RBObj ApplyRB(Vector3 pos, Quaternion rot, Vector3 lv, Vector3 av) {
        if (!_rb) _rb = Go.GetComponent<Rigidbody>();
        // TODO: hermit
        _rb.position = Position= pos;
        _rb.rotation = Rotation= rot;
        _rb.velocity = LVelocity= lv;
        _rb.angularVelocity = AVelocity = av;

        return this;
    }

    public RBObj Clone() {
        return (RBObj) MemberwiseClone();
    }
}

public class Snapshot {
    public RBObj[] cubeStates;
    public RBObj[] playerStates;

    public int CubeCount { get => cubeStates.Length; }
    public int PlayerCount { get => playerStates.Length; }

    private static BitStreamReader _reader = new BitStreamReader();
    private static BitStreamWriter _writer = new BitStreamWriter();

    public Snapshot Clone() {
        Snapshot snap = new Snapshot();
        snap.cubeStates = new RBObj[CubeCount];
        snap.playerStates = new RBObj[PlayerCount];

        for (int i = 0; i < CubeCount; i++) {
            snap.cubeStates[i] = cubeStates[i].Clone();
        }
        for (int i = 0; i < PlayerCount; i++) {
            snap.playerStates[i] = playerStates[i].Clone();
        }

        return snap;
    }

    public static void FromBytes(Snapshot snapshot, byte[] data) {
        _reader.SetBytes(data);
        
        int playerCount = _reader.ReadInt16();
        int cubeCount = _reader.ReadInt16();
        for (int i = 0; i < playerCount; i++) {
            ReadRBObj(snapshot.playerStates);
        }
        for (int i = 0; i < cubeCount; i++) {
            ReadRBObj(snapshot.cubeStates);
        }
    }

    /*
        int player count
        int cube count
        player rbs
        cube rbs

        Not thread-safe
    */
    public static byte[] ToBytes(Snapshot snapshot) {
        _writer.DumpBytes();
        
        int playerCount = 0;
        int cubeCount = 0;
        List<RBObj> rbs = new List<RBObj>();

        foreach (var player in snapshot.playerStates) {
            playerCount++;
            rbs.Add(player);
        }
        foreach (var cube in snapshot.cubeStates) {
            if (cube.Priority > 0) {
                cubeCount++;
                rbs.Add(cube);
            }
        }

        _writer.WriteInt16(playerCount);
        _writer.WriteInt16(cubeCount);
        foreach(var rb in rbs) WriteRBObj(rb);

        return _writer.DumpBytes();
    }

    /*
        id
        pos
        rot
        lv
        av
        priority
    */
    private static void WriteRBObj(RBObj rbobj) {
        _writer.WriteInt16(rbobj.Id);
        _writer.WriteVector3(rbobj.Position);
        _writer.WriteQuaternionRot(rbobj.Rotation);
        _writer.WriteVector3(rbobj.LVelocity);
        _writer.WriteVector3(rbobj.AVelocity);
        _writer.WriteInt16(rbobj.Priority);
    }

    private static void ReadRBObj(RBObj[] rbobjs) {
        int id = _reader.ReadInt16();
        RBObj rbobj = rbobjs[id];
        rbobj.Position = _reader.ReadVector3();
        rbobj.Rotation = _reader.ReadQuaternionRot();
        rbobj.LVelocity = _reader.ReadVector3();
        rbobj.AVelocity = _reader.ReadVector3();
        rbobj.Priority = _reader.ReadInt16();
    }
}