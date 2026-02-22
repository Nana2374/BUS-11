using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusController : MonoBehaviour
{
    public WheelCollider frontLeft, frontRight;
    public WheelCollider rearLeft, rearRight;

    public float motorForce = 80000f;
    public float steerAngle = 45f;
    public float brakeForce = 50000f;
    public float maxSpeed = 50f; // Adjust to your top speed

    public bool playerDriving;

    // Gear System
    public int currentGear = 0; // -1 = Reverse, 0 = Park, 1-4 = Gears
    public float reverseRatio = 0.5f;
    public float[] gearRatios = new float[] { 0f, 0.3f, 0.7f, 1f }; // Park, Gear1, Gear2, Gear3
    public float reverseSpeedLimit = 40f;
    public float[] gearSpeedLimits = new float[] { 0f, 20f, 45f, 70f }; // Park, Gear1, Gear2, Gear3

    Rigidbody rb;
    float motorInput;
    float steerInput;
    float engineBrakeAmount = 0f;
    float currentBrakeAmount = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1.5f, 0);

        // Reduce drag
        rb.drag = 0.5f;
        rb.angularDrag = 3f;
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReverseGear();
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
            rearLeft.brakeTorque = brakeForce * 10f; // Softer park brake
            rearRight.brakeTorque = brakeForce * 10f;
            frontLeft.brakeTorque = brakeForce * 10f;
            frontRight.brakeTorque = brakeForce * 10f;

            // Completely freeze the bus in Park
            rb.constraints = RigidbodyConstraints.FreezePosition |
                            RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationY |
                            RigidbodyConstraints.FreezeRotationZ;
            return;
        }
        else
        {
            // Unfreeze everything when not in Park
            rb.constraints = RigidbodyConstraints.None;
        }

        // Reset engine brake when pressing W or S
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            engineBrakeAmount = 0f;
        }

        // Get current speed in km/h
        float signedSpeed = Vector3.Dot(rb.velocity, transform.forward) * 3.6f;
        float currentSpeed = Mathf.Abs(signedSpeed);

        // Get gear speed limit and ratio based on current gear
        float gearSpeedLimit;
        float gearModifiedForce;

        if (currentGear == -1) // Reverse
        {
            gearSpeedLimit = reverseSpeedLimit;
            gearModifiedForce = motorForce * reverseRatio;
        }
        else // Forward gears 1-4
        {
            gearSpeedLimit = gearSpeedLimits[currentGear];
            gearModifiedForce = motorForce * gearRatios[currentGear];
        }

        // Boost torque during turns
        float turnBoost = 1f + (Mathf.Abs(steerInput) * 0.4f);

        // Gradual brake based on how long S is held
        // Higher = slower increase, Lower = faster increase
        float brakeSmoothing = 1f;

        // Check if braking (S key / down arrow)
        bool isBraking = motorInput < 0;

        // Smoothly calculate brake amount (0 to 1)
        float targetBrakeAmount = isBraking ? 1f : 0f;
        currentBrakeAmount = Mathf.MoveTowards(currentBrakeAmount, targetBrakeAmount, brakeSmoothing * Time.fixedDeltaTime);

        // Calculate the actual brake torque
        float currentBrakeTorque = brakeForce * currentBrakeAmount;

        if (currentGear == -1) // REVERSE GEAR LOGIC
        {
            if (isBraking)
            {
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                //rearLeft.brakeTorque = currentBrakeTorque;
                //rearRight.brakeTorque = currentBrakeTorque;
                frontLeft.brakeTorque = currentBrakeTorque * 0.001f;
                frontRight.brakeTorque = currentBrakeTorque * 0.001f;
            }
            else if (motorInput > 0.1f) // W key pressed
            {
                if (currentSpeed >= gearSpeedLimit)
                {
                    rearLeft.motorTorque = 0f;
                    rearRight.motorTorque = 0f;
                    rearLeft.brakeTorque = brakeForce * 0.1f; // Very light brake at limit
                    rearRight.brakeTorque = brakeForce * 0.1f;
                    frontLeft.brakeTorque = brakeForce * 0.1f;
                    frontRight.brakeTorque = brakeForce * 0.1f;
                }
                else
                {
                    rearLeft.motorTorque = -motorInput * gearModifiedForce * turnBoost;
                    rearRight.motorTorque = -motorInput * gearModifiedForce * turnBoost;
                    rearLeft.brakeTorque = 0f;
                    rearRight.brakeTorque = 0f;
                    frontLeft.brakeTorque = 0f;
                    frontRight.brakeTorque = 0f;
                }
            }
            else
            {
                // No input - gradual engine brake
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;

                // Smoothly increase engine brake over time
                //engineBrakeAmount = Mathf.MoveTowards(engineBrakeAmount, brakeForce * 0.15f, 500f * Time.fixedDeltaTime);

                //rearLeft.brakeTorque = engineBrakeAmount;
                //rearRight.brakeTorque = engineBrakeAmount;
                //frontLeft.brakeTorque = engineBrakeAmount * 0.00001f;
                //frontRight.brakeTorque = engineBrakeAmount * 0.00001f;
            }
        }
        else // FORWARD GEAR LOGIC
        {
            if (isBraking)
            {
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                // Gradual braking - rear takes 60%, front takes 40% (realistic bus braking)
                //rearLeft.brakeTorque = currentBrakeTorque * 0.006f;
                //rearRight.brakeTorque = currentBrakeTorque * 0.006f;
                frontLeft.brakeTorque = currentBrakeTorque * 0.004f;
                frontRight.brakeTorque = currentBrakeTorque * 0.004f;
            }
            else if (motorInput > 0.1f)
            {
                // STRICT SPEED LIMITING
                if (currentSpeed >= gearSpeedLimit)
                {
                    rearLeft.motorTorque = 0f;
                    rearRight.motorTorque = 0f;

                    float overspeed = currentSpeed - gearSpeedLimit;
                    // Gentler speed limit braking
                    float brakingForce = brakeForce * Mathf.Clamp(overspeed / 10f, 0.1f, 0.5f);

                    rearLeft.brakeTorque = brakingForce;
                    rearRight.brakeTorque = brakingForce;
                    frontLeft.brakeTorque = brakingForce * 0.5f;
                    frontRight.brakeTorque = brakingForce * 0.5f;
                }
                else if (currentSpeed >= gearSpeedLimit * 0.8f)
                {
                    float proximityToLimit = (currentSpeed - (gearSpeedLimit * 0.8f)) / (gearSpeedLimit * 0.2f);
                    float powerReduction = 1f - (proximityToLimit * 0.7f);

                    rearLeft.motorTorque = motorInput * gearModifiedForce * turnBoost * powerReduction;
                    rearRight.motorTorque = motorInput * gearModifiedForce * turnBoost * powerReduction;

                    rearLeft.brakeTorque = 0f;
                    rearRight.brakeTorque = 0f;
                    frontLeft.brakeTorque = 0f;
                    frontRight.brakeTorque = 0f;
                }
                else
                {
                    rearLeft.motorTorque = motorInput * gearModifiedForce * turnBoost;
                    rearRight.motorTorque = motorInput * gearModifiedForce * turnBoost;

                    rearLeft.brakeTorque = 0f;
                    rearRight.brakeTorque = 0f;
                    frontLeft.brakeTorque = 0f;
                    frontRight.brakeTorque = 0f;
                }
            }
            else
            {
                // No input - very gentle auto-brake (heavy bus rolls slowly)
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                rearLeft.brakeTorque = brakeForce * 0f;
                rearRight.brakeTorque = brakeForce * 0f;
                //frontLeft.brakeTorque = brakeForce * 0.00001f;
                //frontRight.brakeTorque = brakeForce * 0.00001f;
            }
        }

        // Calculate speed-based steering
        float speed = rb.velocity.magnitude;
        //float steerMultiplier = Mathf.Clamp(1f - (speed / maxSpeed) * 0.5f, 0.5f, 1f);

        float currentSteerAngle = steerInput * steerAngle; //* steerMultiplier;

        frontLeft.steerAngle = currentSteerAngle;
        frontRight.steerAngle = currentSteerAngle;

        // Rear wheels steer opposite (slight angle)
        //rearLeft.steerAngle = -steerInput * (steerAngle * 0.3f);
        //rearRight.steerAngle = -steerInput * (steerAngle * 0.3f);
    }

    void UpShift()
    {
        if (currentGear < 4)
        {
            // Get current speed
            float currentSpeed = rb.velocity.magnitude * 3.6f;
            float currentGearLimit = gearSpeedLimits[currentGear];

            // Check if we're at least 80% of current gear's speed limit before allowing upshift
            float minSpeedToShift = currentGearLimit * 0.8f;

            if (currentGear == 0) // Special case for Park -> Gear 1
            {
                currentGear++;
                Debug.Log("Shifted UP to Gear: " + currentGear);
            }
            else if (currentSpeed >= minSpeedToShift)
            {
                currentGear++;
                Debug.Log($"Shifted UP to Gear: {currentGear} (Speed: {currentSpeed:F0} km/h)");
            }
            else
            {
                Debug.Log($"Cannot upshift! Reach {minSpeedToShift:F0} km/h first. Current: {currentSpeed:F0} km/h");
            }
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

    void ReverseGear()
    {
        currentGear = -1;
        Debug.Log("Shifted to REVERSE (Max: 40 km/h)");
    }

    // Optional: Display current gear on screen
    void OnGUI()
    {
        if (playerDriving)
        {
            string gearDisplay = currentGear == -1 ? "R" : (currentGear == 0 ? "P" : currentGear.ToString());
            GUI.Label(new Rect(10, 10, 200, 30), "Gear: " + gearDisplay,
                new GUIStyle() { fontSize = 24, normal = new GUIStyleState() { textColor = Color.white } });

            float speed = rb.velocity.magnitude * 3.6f;
            GUI.Label(new Rect(10, 40, 200, 30), "Speed: " + speed.ToString("F0") + " km/h",
                new GUIStyle() { fontSize = 20, normal = new GUIStyleState() { textColor = Color.white } });

            // Show speed limit for current gear with color coding
            if (currentGear != 0)
            {
                float limit = currentGear == -1 ? reverseSpeedLimit : gearSpeedLimits[currentGear];
                Color limitColor = Color.yellow;

                if (speed >= limit) limitColor = Color.red;
                else if (speed >= limit * 0.8f) limitColor = new Color(1f, 0.5f, 0f); // Orange

                GUI.Label(new Rect(10, 70, 200, 30), $"Limit: {limit} km/h",
                    new GUIStyle() { fontSize = 18, normal = new GUIStyleState() { textColor = limitColor } });
            }
        }
    }
}
