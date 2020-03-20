using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBObjHolder : MonoBehaviour
{
    public RBObj rBObj = null;

    // Update is called once per frame
    void Update()
    {
        rBObj.Priority += 1; // Passively increase the priority of everything every tick
    }

}
