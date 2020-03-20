using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{

    void OnCollisionEnter(Collision collision) {
        OnCollisionStay(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name.Equals("Cube_Physics(Clone)")) {
            // Debug.Log(collision.gameObject.name); 
            RBObjHolder holder = collision.gameObject.GetComponent(typeof(RBObjHolder)) as RBObjHolder;
            holder.rBObj.Priority += 100; // Give the cube higher priority.
            Debug.Log("Cube priority is " + holder.rBObj.Priority);
        }
    }
}
    