using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public float _velocity = 10;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddForce(_velocity * Input.GetAxis("Horizontal"),
            0f,
            _velocity * Input.GetAxis("Vertical"));
         if (Input.GetKeyDown ("space")) {
             Vector3 up = transform.TransformDirection (Vector3.up);
             rb.AddForce(up * 5, ForceMode.Impulse);
         }
    }
}
