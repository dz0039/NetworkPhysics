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
    public Rigidbody Rigidbody { get => _rb; }

    public bool IsActive {get => Go.activeInHierarchy;}

    public RBObj SetActive(bool val) {
        // TODO: RB errors?
        Go.SetActive(val);
        return this;
    }

    public RBObj ApplyRB(Vector3 pos, Quaternion rot, Vector3 lv, Vector3 av) {
        if (!_rb) _rb = Go.GetComponent<Rigidbody>();
        // TODO: hermit
        _rb.position = pos;
        _rb.rotation = rot;
        _rb.velocity = lv;
        _rb.angularVelocity = AVelocity = av;

        return this;
    }

    public void UpdateFromRigid() {
        if (!_rb) _rb = Go.GetComponent<Rigidbody>();
        Position = _rb.position;
        Rotation = _rb.rotation;
        LVelocity = _rb.velocity;
        AVelocity = _rb.angularVelocity;
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

    public Snapshot() { }

    public Snapshot(List<RBObj> playersList, List<RBObj> cubesList) {
        cubeStates = cubesList.ToArray();
        playerStates = playersList.ToArray();
    }

    public List<RBObj> getPriorityCubes(int maxCubes)
    {
        List<RBObj> priority = new List<RBObj>();
        priority.AddRange(cubeStates);
        priority.Sort((x, y) => (x.Priority - y.Priority));

        if (priority.Count > maxCubes)
        {
            priority.RemoveRange(maxCubes, priority.Count - maxCubes);
        }

        return priority;
    }

    public void clearPriority(List<RBObj> toClear) {
        foreach (RBObj rBObj in toClear) {
            rBObj.Priority = 0;
        }
    }

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

    public void UpdateFromRigid() {
        foreach (RBObj rb in cubeStates) {
            rb.UpdateFromRigid();
        }
        // null?
        foreach (RBObj rb in playerStates) {
            rb.UpdateFromRigid();
        }
    }

    public static Snapshot FromBytes(byte[] data) {
        BitStreamReader reader = new BitStreamReader();
        reader.SetBytes(data);

        int playerCount = reader.ReadInt16();
        int cubeCount = reader.ReadInt16();
        List<RBObj> players = new List<RBObj>();
        List<RBObj> cubes = new List<RBObj>();
        for (int i = 0; i < playerCount; i++) {
            RBObj read = ReadRBObj(reader);
            players.Add(read);
        }
        for (int i = 0; i < cubeCount; i++) {
            RBObj read = ReadRBObj(reader);
            cubes.Add(read);
        }
        return new Snapshot(players, cubes);
    }

    /*
        int player count
        int cube count
        player rbs
        cube rbs

        Not thread-safe
    */
    public static byte[] ToBytes(Snapshot snapshot) {
        BitStreamWriter writer = new BitStreamWriter();
        
        writer.WriteInt16(snapshot.playerStates.Length);
        writer.WriteInt16(snapshot.cubeStates.Length);
        foreach (var player in snapshot.playerStates)
        {
            WriteRBObj(writer, player);
        }
        foreach (var cube in snapshot.cubeStates)
        {
            WriteRBObj(writer, cube);
        }

        return writer.DumpBytes();
    }

    /*
        id
        pos
        rot
        lv
        av
        priority
    */
    private static void WriteRBObj(BitStreamWriter writer, RBObj rbobj) {
        writer.WriteInt16(rbobj.Id);
        writer.WriteVector3(rbobj.Position);
        writer.WriteQuaternionRot(rbobj.Rotation);
        writer.WriteVector3(rbobj.LVelocity);
        writer.WriteVector3(rbobj.AVelocity);
        writer.WriteInt32(rbobj.Priority);
    }

    private static RBObj ReadRBObj(BitStreamReader reader)
    {
        RBObj rbobj = new RBObj();
        rbobj.Id = reader.ReadInt16();
        rbobj.Position = reader.ReadVector3();
        rbobj.Rotation = reader.ReadQuaternionRot();
        rbobj.LVelocity = reader.ReadVector3();
        rbobj.AVelocity = reader.ReadVector3();
        rbobj.Priority = reader.ReadInt32();
        return rbobj;
    }
}