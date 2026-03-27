using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusController : MonoBehaviour
{
    public WheelCollider frontLeft, frontRight;
    public WheelCollider rearLeft, rearRight;

    public float motorForce = 80000f;
    public float steerAngle = 60f;
    public float brakeForce = 50000f;
    public float maxSpeed = 50f; // Adjust to your top speed

    public bool playerDriving;

    // Gear System
    public int currentGear = 0; // -1 = Reverse, 0 = Park, 1-3 = Gears
    public float reverseRatio = 0.5f;
    public float[] gearRatios = new float[] { 0f, 0.3f, 0.7f, 1f }; // Park, Gear1, Gear2, Gear3
    public float reverseSpeedLimit = 40f;
    public float[] gearSpeedLimits = new float[] { 0f, 20f, 45f, 70f }; // Park, Gear1, Gear2, Gear3

    public Rigidbody rb;
    float motorInput;
    float steerInput;
    float engineBrakeAmount = 0f;
    float currentBrakeAmount = 0f;

    [Header("Audio")]
    public AudioSource engineSource;
    public AudioSource brakeSource;

    public AudioClip engineIdleClip;
    public AudioClip engineDriveClip;
    public AudioClip brakeClip;

    public float minPitch = 0.8f;
    public float maxPitch = 2.0f;

    [Header("Ghost Control")]
    public float ghostSteerForce = 10f; // how strong the pull is
    public float ghostAccelerationForce = 10f;
    private float ghostSteerTimer = 0f;
    private bool ghostActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1.5f, 0);

        // Reduce drag
        rb.drag = 0.5f;
        rb.angularDrag = 1.5f;

        // START IN PARK - freeze the bus immediately
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (engineSource != null)
        {
            AudioManager.Instance.RegisterSFXSource(engineSource);
        }

        if (brakeSource != null)
        {
            AudioManager.Instance.RegisterSFXSource(brakeSource);
        }

        engineSource.clip = engineIdleClip;
        engineSource.loop = true;
        engineSource.Play();
    }

    void Update()
    {
        if (!playerDriving)
        {
            currentGear = 0; // Ensure we're in Park when not driving
            return;
        }

        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        // Apply ghost influence (adds to player input)
        if (ghostActive)
        {
            // Ghost presses "W"
            motorInput = Mathf.Max(motorInput, ghostAccelerationForce);
            steerInput += ghostSteerForce * Mathf.Clamp01(1f - (ghostSteerTimer / 5f));
        }

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
        if (ghostActive)
        {
            ghostSteerTimer -= Time.fixedDeltaTime;

            // Remove brakes so ghost can accelerate freely
            rearLeft.brakeTorque = 0f;
            rearRight.brakeTorque = 0f;
            frontLeft.brakeTorque = 0f;
            frontRight.brakeTorque = 0f;

            if (ghostSteerTimer <= 0f)
            {
                ghostActive = false;
            }
        }

        // Apply Park constraints even when player isn't driving
        if (!playerDriving || currentGear == 0)
        {
            if (currentGear == 0)
            {
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                rearLeft.brakeTorque = brakeForce * 10f;
                rearRight.brakeTorque = brakeForce * 10f;
                frontLeft.brakeTorque = brakeForce * 10f;
                frontRight.brakeTorque = brakeForce * 10f;

                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            if (!playerDriving) return; // Return AFTER applying Park
        }
        else
        {
            // When driving and NOT in Park: only lock tilt rotations
            rb.constraints = RigidbodyConstraints.None;
        }

        if (!playerDriving) return;

        // Get current speed in km/h
        float signedSpeed = Vector3.Dot(rb.velocity, transform.forward) * 3.6f;
        float currentSpeed = Mathf.Abs(signedSpeed);

        // Boost torque during turns
        float turnBoost = 1f + (Mathf.Abs(steerInput) * 2.5f);

        // Boost torque during turns - MORE boost at higher speeds
        float baseTurnBoost = 1f + (Mathf.Abs(steerInput) * 2.0f);

        // Extra boost if speed is dropping during turn
        float speedBoost = 1f;
        if (Mathf.Abs(steerInput) > 0.3f && currentSpeed < 30f)
        {
            speedBoost = 1.5f; // 50% extra power when turning slowly
        }

        //float turnBoost = baseTurnBoost * speedBoost;

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
            rb.constraints = RigidbodyConstraints.FreezeAll; // Lock EVERYTHING
            return;
        }
        else
        {
            // Unfreeze everything when not in Park
            rb.constraints = RigidbodyConstraints.None;
        }

        // NEW: If speed exceeds gear limit, apply braking
        float gearSpeedLimit = currentGear == -1 ? reverseSpeedLimit : gearSpeedLimits[currentGear];

        // Reset engine brake when pressing W or S
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            engineBrakeAmount = 0f;
        }

        // Get gear speed limit and ratio based on current gear
        //float gearSpeedLimit;
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

        // At low speeds: full steering (60 degrees)
        // At high speeds: reduced steering (30 degrees)
        float speedFactor = Mathf.Clamp01(currentSpeed / 40f); // 0 at 0 km/h, 1 at 40+ km/h
        float dynamicSteerAngle = Mathf.Lerp(steerAngle, steerAngle * 0.5f, speedFactor);

        float currentSteerAngle = steerInput * dynamicSteerAngle;

        frontLeft.steerAngle = currentSteerAngle;
        frontRight.steerAngle = currentSteerAngle;

        // Calculate speed-based steering
        //float speed = rb.velocity.magnitude;
        //float steerMultiplier = Mathf.Clamp(1f - (speed / maxSpeed) * 0.5f, 0.5f, 1f);

        //float currentSteerAngle = steerInput * steerAngle; //* steerMultiplier;

        //frontLeft.steerAngle = currentSteerAngle;
        //frontRight.steerAngle = currentSteerAngle;

        // Rear wheels steer opposite (slight angle)
        //rearLeft.steerAngle = -steerInput * (steerAngle * 0.3f);
        //rearRight.steerAngle = -steerInput * (steerAngle * 0.3f);

        // Get speed
        float speed = rb.velocity.magnitude;

        // Normalize speed (0 to 1)
        float speedPercent = Mathf.Clamp01(speed / maxSpeed);

        // Adjust pitch based on speed
        engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);

        // Switch between idle and driving sound
        bool isTryingToMove = Mathf.Abs(motorInput) > 0.1f && currentGear != 0;

        if (currentGear == 0)
        {
            if (engineSource.clip != engineIdleClip)
            {
                engineSource.Stop(); // Stop current sound
                engineSource.clip = engineIdleClip;
                engineSource.Play();
            }
        }
        else if (isTryingToMove)
        {
            // Driving - play drive sound
            if (engineSource.clip != engineDriveClip)
            {
                engineSource.clip = engineDriveClip;
                engineSource.Play();
            }
        }
        else
        {
            // Not driving but not in Park - play idle sound
            if (engineSource.clip != engineIdleClip)
            {
                engineSource.clip = engineIdleClip;
                engineSource.Play();
            }
        }
        ClampTilt();

        //ApplyRoadBumps();
    }

    void UpShift()
    {
        if (currentGear == -1) // Reverse
        {
            currentGear = 1; // Go to Gear 1
            Debug.Log("Shifted to Gear 1");
            return;
        }
        else if (currentGear == 0) // Park
        {
            currentGear = 1; // Go to Gear 1
            Debug.Log("Shifted UP to Gear: 1");
            return;
        }

        if (currentGear < 3)
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
        Debug.Log("Shifted to REVERSE (Max: 15 km/h)");
    }

    public void TriggerGhostEvent(float duration, float force, float accelForce)
    {
        ghostActive = true;
        ghostSteerTimer = duration;
        ghostSteerForce = force;
        ghostAccelerationForce = accelForce;
    }

    void ClampTilt()
    {
        Vector3 angles = rb.rotation.eulerAngles;

        float tiltX = NormalizeAngle(angles.x);
        float tiltZ = NormalizeAngle(angles.z);

        float maxTilt = 15f; // tweak this (10–20 good range)

        tiltX = Mathf.Clamp(tiltX, -maxTilt, maxTilt);
        tiltZ = Mathf.Clamp(tiltZ, -maxTilt, maxTilt);

        rb.rotation = Quaternion.Euler(tiltX, angles.y, tiltZ);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    /*void ApplyRoadBumps()
    {
        float speed = rb.velocity.magnitude;

        if (speed < 1f) return;

        float bumpStrength = 300000f; // tweak
        float bumpFrequency = 8f;   // tweak

        float bump = Mathf.PerlinNoise(Time.time * bumpFrequency, 0f) - 0.5f;

        Vector3 force = transform.up * bump * bumpStrength;

        rb.AddForce(force * Time.fixedDeltaTime);

        Debug.Log("Applying bump force: " + force.magnitude);
    }*/

    // Optional: Display current gear on screen
    /*void OnGUI()
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
    }*/
}
