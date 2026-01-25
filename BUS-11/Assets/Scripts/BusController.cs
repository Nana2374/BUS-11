using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusController : MonoBehaviour
{
    public float motorForce = 150f;
    public float turnForce = 50f;

    public bool playerDriving; // controlled by seat script
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!playerDriving) return;

        float move = Input.GetAxis("Vertical");   // W/S
        float turn = Input.GetAxis("Horizontal"); // A/D

        // Forward movement
        rb.AddForce(transform.forward * move * motorForce, ForceMode.Acceleration);

        // Turning
        Quaternion deltaRotation = Quaternion.Euler(0f, turn * turnForce * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
