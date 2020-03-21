using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBObjHolder : MonoBehaviour
{
    public RBObj rBObj;

    static float max_vel = 0.0f;

    // Update is called once per frame
    void Update()
    {
        // Increment priority
        rBObj.Priority++;
    }
}
