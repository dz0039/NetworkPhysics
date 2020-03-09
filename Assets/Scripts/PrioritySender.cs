using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * The host should start hosting a game session.
 * It should manage the game state by sending and 
 * receiving from any clients that connects.
 */
public class PrioritySender {

    private Dictionary<RBObj, double> priorityAccumulator;

    public PrioritySender() {
        this.priorityAccumulator = new Dictionary<RBObj, double>();
    }

    public void updateAccumulator() {
        Snapshot snapshot = Game.Instance.GetSnapshot();

        foreach (RBObj rbObj in snapshot.playerStates)
        {
            if (!priorityAccumulator.ContainsKey(rbObj))
            {
                priorityAccumulator[rbObj] = 0;
            }
            priorityAccumulator[rbObj] += rbObj.Priority;
        }

        foreach (RBObj rbObj in snapshot.cubeStates) {
            if (!priorityAccumulator.ContainsKey(rbObj)) {
                priorityAccumulator[rbObj] = 0;
            }
            priorityAccumulator[rbObj] += rbObj.Priority;
        }
    }

    public List<RBObj> getWithPriority(int maxBytes) {
        var sorted = new List<RBObj>(priorityAccumulator.Keys);
        sorted.Sort((x, y) => sortByPriority(x, y));

        const int rbObjSize = 50;

        if (sorted.Count > maxBytes / rbObjSize) {
            sorted.RemoveRange(maxBytes / rbObjSize, sorted.Count - maxBytes / rbObjSize);
        }
        return null;
    }

    private int sortByPriority(RBObj x, RBObj y) {
        float priorityDif = (float) (priorityAccumulator[x] - priorityAccumulator[y]);
        if (Mathf.Abs(priorityDif) < .00001)
        {
            return 0;
        }
        else if (priorityDif < 0)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

}