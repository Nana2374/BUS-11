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

    // Gear System
    public int currentGear = 0; // 0 = Park, 1-4 = Gears
    public float[] gearRatios = new float[] { 0f, 0.3f, 0.8f, 1.1f, 1.3f }; // Park, Gear1, Gear2, Gear3, Gear4
    public float[] gearSpeedLimits = new float[] { 0f, 20f, 35f, 50f, 70f }; // Max speed per gear in km/h

    Rigidbody rb;
    float motorInput;
    float steerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1.5f, 0);

        // Reduce drag
        rb.drag = 0.05f;
        rb.angularDrag = 0.05f;
    }

    void Update()
    {
        if (!playerDriving) return;

        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        // Gear shifting
        if (Input.GetKeyDown(KeyCode.E))
        {
            UpShift();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DownShift();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ParkGear();
        }
    }

    void FixedUpdate()
    {
        if (!playerDriving) return;

        // Can't move in Park
        if (currentGear == 0)
        {
            rearLeft.motorTorque = 0f;
            rearRight.motorTorque = 0f;
            rearLeft.brakeTorque = brakeForce;
            rearRight.brakeTorque = brakeForce;
            frontLeft.brakeTorque = brakeForce;
            frontRight.brakeTorque = brakeForce;
            return;
        }

        // Get current speed in km/h
        float currentSpeed = rb.velocity.magnitude * 3.6f;
        float gearSpeedLimit = gearSpeedLimits[currentGear];

        // Calculate gear-modified motor force
        float gearModifiedForce = motorForce * gearRatios[currentGear];

        // Reduce power as we approach speed limit
        float speedLimitFactor = 1f;
        if (currentSpeed >= gearSpeedLimit * 0.9f) // Start reducing at 90% of limit
        {
            speedLimitFactor = Mathf.Clamp01(1f - ((currentSpeed - gearSpeedLimit * 0.9f) / (gearSpeedLimit * 0.1f)));
        }

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
            // STRICT SPEED LIMITING
            if (currentSpeed >= gearSpeedLimit)
            {
                // AT OR OVER LIMIT - Cut all power and brake hard
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;

                // Strong braking to enforce limit
                float overspeed = currentSpeed - gearSpeedLimit;
                float brakingForce = brakeForce * Mathf.Clamp(overspeed / 5f, 0.3f, 1f);

                rearLeft.brakeTorque = brakingForce;
                rearRight.brakeTorque = brakingForce;
                frontLeft.brakeTorque = brakingForce * 0.5f;
                frontRight.brakeTorque = brakingForce * 0.5f;

                Debug.Log($"SPEED LIMITED! Current: {currentSpeed:F1} | Limit: {gearSpeedLimit}");
            }
            else if (currentSpeed >= gearSpeedLimit * 0.8f)
            {
                // APPROACHING LIMIT (80-100%) - Reduce power progressively
                float proximityToLimit = (currentSpeed - (gearSpeedLimit * 0.8f)) / (gearSpeedLimit * 0.2f);
                float powerReduction = 1f - (proximityToLimit * 0.7f); // Reduce up to 70%

                rearLeft.motorTorque = motorInput * gearModifiedForce * turnBoost * powerReduction;
                rearRight.motorTorque = motorInput * gearModifiedForce * turnBoost * powerReduction;

                // Release brakes
                rearLeft.brakeTorque = 0f;
                rearRight.brakeTorque = 0f;
                frontLeft.brakeTorque = 0f;
                frontRight.brakeTorque = 0f;
            }
            else
            {
                // UNDER 80% OF LIMIT - Full power
                rearLeft.motorTorque = motorInput * gearModifiedForce * turnBoost;
                rearRight.motorTorque = motorInput * gearModifiedForce * turnBoost;

                // Release brakes
                rearLeft.brakeTorque = 0f;
                rearRight.brakeTorque = 0f;
                frontLeft.brakeTorque = 0f;
                frontRight.brakeTorque = 0f;
            }
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

    void UpShift()
    {
        if (currentGear < 4)
        {
            currentGear++;
            Debug.Log("Shifted UP to Gear: " + currentGear);
        }
        else
        {
            Debug.Log("Already in highest gear!");
        }
    }

    void DownShift()
    {
        if (currentGear > 1)
        {
            currentGear--;
            Debug.Log("Shifted DOWN to Gear: " + currentGear);
        }
        else if (currentGear == 1)
        {
            Debug.Log("Already in lowest gear! Press P for Park.");
        }
    }

    void ParkGear()
    {
        currentGear = 0;
        Debug.Log("Shifted to PARK");
    }

    // Optional: Display current gear on screen
    void OnGUI()
    {
        if (playerDriving)
        {
            string gearDisplay = currentGear == 0 ? "P" : currentGear.ToString();
            GUI.Label(new Rect(10, 10, 200, 30), "Gear: " + gearDisplay,
                new GUIStyle() { fontSize = 24, normal = new GUIStyleState() { textColor = Color.white } });

            float speed = rb.velocity.magnitude * 3.6f; // Convert to km/h
            GUI.Label(new Rect(10, 40, 200, 30), "Speed: " + speed.ToString("F0") + " km/h",
                new GUIStyle() { fontSize = 20, normal = new GUIStyleState() { textColor = Color.white } });

            // Show speed limit for current gear with color coding
            if (currentGear > 0)
            {
                float limit = gearSpeedLimits[currentGear];
                Color limitColor = Color.yellow;

                if (speed >= limit) limitColor = Color.red;
                else if (speed >= limit * 0.8f) limitColor = new Color(1f, 0.5f, 0f); // Orange

                GUI.Label(new Rect(10, 70, 200, 30), $"Limit: {limit} km/h",
                    new GUIStyle() { fontSize = 18, normal = new GUIStyleState() { textColor = limitColor } });
            }
        }
    }
}
