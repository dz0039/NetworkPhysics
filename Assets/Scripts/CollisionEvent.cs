using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("Cube_Physics(Clone)")) {
            // Debug.Log(collision.gameObject.name); 
            RBObjHolder holder = collision.gameObject.GetComponent(typeof(RBObjHolder)) as RBObjHolder;
            holder.rBObj.Priority += 100; // Give the cube higher priority.
            Debug.Log("Cube priority is " + holder.rBObj.Priority);
        }
    }
}
    