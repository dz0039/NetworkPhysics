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
    public int Owner { get; set; }

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

        foreach (RBObj rb in cubeStates) {
            if (rb.Owner == Game.Instance.getMainPlayerID() || Game.Instance.isGameServer())
            {
                priority.Add(rb);
            }
        }

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
        id : int 8
        pos : x,y in (-16,16)m (5 b); z in [-1,3]m(2 b); 512(9b) positions per meter
        rot : 3 three
        lv : -1024 1024, int6: 64 meters, int6: 64 per meter
        av : same
        priority : int32
    */
    private readonly static Vector3 POS_OFFSET_W = new Vector3(16,1,16);
    private readonly static Vector3 POS_OFFSET_R = new Vector3(16,3,16);
    private readonly static Vector3 VEL_OFFSET = new Vector3(16,16,16);
    private static void WriteRBObj(BitStreamWriter writer, RBObj rbobj) {
        writer.WriteInt8(rbobj.Id);
        WriteCompressedVector3(writer, rbobj.Position + POS_OFFSET_W,
             5,9,2,9,5,9);
        writer.WriteQuaternionRot(rbobj.Rotation);
        WriteCompressedVector3(writer, rbobj.LVelocity + VEL_OFFSET,
             5,6,5,6,5,6);
        WriteCompressedVector3(writer, rbobj.AVelocity + VEL_OFFSET,
             5,6,5,6,5,6);
        writer.WriteInt32(rbobj.Priority);
    }

    private static RBObj ReadRBObj(BitStreamReader reader)
    {
        RBObj rbobj = new RBObj();
        rbobj.Id = reader.ReadInt8();
        rbobj.Position = ReadCompressedVector3(reader,
             5,9,2,9,5,9)-POS_OFFSET_R;
        rbobj.Rotation = reader.ReadQuaternionRot();
        rbobj.LVelocity = ReadCompressedVector3(reader,
             5,6,5,6,5,6)-VEL_OFFSET;
        rbobj.AVelocity = ReadCompressedVector3(reader,
             5,6,5,6,5,6)-VEL_OFFSET;
        rbobj.Priority = reader.ReadInt32();
        return rbobj;
    }

    private static int IntPow(int a, int b)
    {
      int result = 1;
      for (int i = 0; i < b; i++)
        result *= a;
      return result;
    }

    // assume all positive
    private static void WriteCompressedVector3(BitStreamWriter writer, Vector3 vec3,int xbits_i, int xbits_f, int ybits_i, int ybits_f, int zbits_i, int zbits_f) {
        int xi = (int)vec3.x;
        int xf = (int)((vec3.x-(float)xi) / (1.0f / (float)IntPow(2,xbits_f)));
        writer.WriteInt(xi,xbits_i);
        writer.WriteInt(xf,xbits_f);
        int yi = (int)vec3.y;
        int yf = (int)((vec3.y-(float)yi) / (1.0f / (float)IntPow(2,ybits_f)));
        writer.WriteInt(yi,ybits_i);
        writer.WriteInt(yf,ybits_f);
        int zi = (int)vec3.z;
        int zf = (int)((vec3.z-(float)zi) / (1.0f / (float)IntPow(2,zbits_f)));
        writer.WriteInt(zi,zbits_i);
        writer.WriteInt(zf,zbits_f);
    }
    private static Vector3 ReadCompressedVector3(BitStreamReader reader,int xbits_i, int xbits_f, int ybits_i, int ybits_f, int zbits_i, int zbits_f) {
        Vector3 res;
        int xi = reader.ReadInt(xbits_i);
        int xf = reader.ReadInt(xbits_f);
        res.x = xi + (xf * (1.0f / (float)IntPow(2,xbits_f)));
        int yi = reader.ReadInt(ybits_i);
        int yf = reader.ReadInt(ybits_f);
        res.y = yi + (yf * (1.0f / (float)IntPow(2,ybits_f)));
        int zi = reader.ReadInt(zbits_i);
        int zf = reader.ReadInt(zbits_f);
        res.z = zi + (zf * (1.0f / (float)IntPow(2,zbits_f)));
        return res;
    }
}