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
        // Only the main player can add priority to a cube
        if (gameObject.GetComponent<RBObjHolder>().rBObj.Id == Game.Instance.getMainPlayerID())
        {
            // Only add priority to cubes
            if (collision.gameObject.name.Equals("Cube_Physics(Clone)"))
            {
                // Give the cube higher priority.
                RBObj rBObj = collision.gameObject.GetComponent<RBObjHolder>().rBObj;
                // Debug.Log(collision.gameObject.name);
                rBObj.Priority += 100;
                // Debug.Log(rBObj.Priority);
            }
        }
    }
}
