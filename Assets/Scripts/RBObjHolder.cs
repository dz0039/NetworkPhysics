using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBObjHolder : MonoBehaviour
{
    public RBObj rBObj;

    // Update is called once per frame
    void Update()
    {
        // Increment priority
        rBObj.Priority++;
    }
}
