using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public float velocity = 5;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(velocity * Input.GetAxis("Horizontal"));
        rb.AddForce(velocity * Input.GetAxis("Horizontal"),
            0f,
            velocity * Input.GetAxis("Vertical"));
    }
}
