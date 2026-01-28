using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusController : MonoBehaviour
{
    public WheelCollider frontLeft, frontRight;
    public WheelCollider rearLeft, rearRight;

    public float motorForce = 6000f;
    public float steerAngle = 25f;
    public float brakeForce = 30000f;

    public bool playerDriving;

    Rigidbody rb;
    float motorInput;
    float steerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1.5f, 0);
    }

    void Update()
    {
        if (!playerDriving) return;

        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        if (!playerDriving) return;

        // Boost torque during turns
        float turnBoost = 1f + (Mathf.Abs(steerInput) * 0.5f); // 50% boost at full turn

        // Rear wheel drive (realistic bus)
        rearLeft.motorTorque = motorInput * motorForce * turnBoost;
        rearRight.motorTorque = motorInput * motorForce * turnBoost;

        // Steering front wheels
        frontLeft.steerAngle = steerInput * steerAngle;
        frontRight.steerAngle = steerInput * steerAngle;

        // Auto brake when no input
        if (Mathf.Abs(motorInput) < 0.1f)
        {
            rearLeft.brakeTorque = brakeForce;
            rearRight.brakeTorque = brakeForce;
        }
        else
        {
            rearLeft.brakeTorque = 0f;
            rearRight.brakeTorque = 0f;
        }
    }
}
