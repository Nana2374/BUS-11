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
    public float maxSpeed = 50f; // Adjust to your top speed


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
        float turnBoost = 1f + (Mathf.Abs(steerInput) * 0.6f); // 50% boost at full turn

        // Check if braking (S key / down arrow)
        bool isBraking = motorInput < 0;

        if (isBraking)
        {
            // Apply brakes when pressing S
            rearLeft.motorTorque = 0f;
            rearRight.motorTorque = 0f;
            rearLeft.brakeTorque = brakeForce;
            rearRight.brakeTorque = brakeForce;
            frontLeft.brakeTorque = brakeForce;
            frontRight.brakeTorque = brakeForce;
        }
        else if (motorInput > 0.1f)
        {
            // Rear wheel drive (realistic bus)
            rearLeft.motorTorque = motorInput * motorForce * turnBoost;
        rearRight.motorTorque = motorInput * motorForce * turnBoost;

            // Release brakes
            rearLeft.brakeTorque = 0f;
            rearRight.brakeTorque = 0f;
            frontLeft.brakeTorque = 0f;
            frontRight.brakeTorque = 0f;
        }
        else
        {
            // No input - gentle auto-brake
            rearLeft.motorTorque = 0f;
            rearRight.motorTorque = 0f;
            rearLeft.brakeTorque = brakeForce * 0.3f;
            rearRight.brakeTorque = brakeForce * 0.3f;
        }

        // Calculate speed-based steering
        float speed = rb.velocity.magnitude;
        float steerMultiplier = Mathf.Clamp(1f - (speed / maxSpeed) * 0.5f, 0.5f, 1f);

        float currentSteerAngle = steerInput * steerAngle * steerMultiplier;

        frontLeft.steerAngle = currentSteerAngle;
        frontRight.steerAngle = currentSteerAngle;

        // Rear wheels steer opposite (slight angle)
        rearLeft.steerAngle = -steerInput * (steerAngle * 0.3f);
        rearRight.steerAngle = -steerInput * (steerAngle * 0.3f);

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
