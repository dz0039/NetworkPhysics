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
            Debug.Log(collision.gameObject.name); // Give the cube higher priority.
        }
    }
}
