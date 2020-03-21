using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void OnCollisionEnter(Collision collision)
    {
        OnCollisionStay(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        // Only add priority to cubes
        if (collision.gameObject.name.Equals("Cube_Physics(Clone)"))
        {
            RBObj rBObj = collision.gameObject.GetComponent<RBObjHolder>().rBObj;
            // Give the cube higher priority.
            
            // Debug.Log(collision.gameObject.name);
            rBObj.Priority += 100;
           
            // Debug.Log(rBObj.Priority);
            
            // unused
            // rBObj.Owner = gameObject.GetComponent<RBObjHolder>().rBObj.Id;
        }
    }
}
