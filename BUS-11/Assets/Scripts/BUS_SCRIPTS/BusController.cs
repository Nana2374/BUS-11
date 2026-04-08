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
    //public float maxSpeed = 50f; // Adjust to your top speed
    public float maxForwardSpeed = 50f;
    public float maxReverseSpeed = 15f;
    public float accelerationTime = 8f;   // Seconds to reach full torque from 0

    public bool playerDriving;

    public int currentGear = 0; // 0 = Park, 
    public float driveSpeedLimit = 40f;
    public float reverseRatio = 0.4f;
    public float reverseSpeedLimit = 15f;

    public Rigidbody rb;
    float motorInput;
    float steerInput;
    float engineBrakeAmount = 0f;
    float currentBrakeAmount = 0f;
    float currentTorqueRamp = 0f;         // Ramps from 0 → 1 for gradual acceleration

    [Header("Door Reference")]
    public BusDoors busDoors;

    [Header("Audio")]
    public AudioSource engineSource;
    public AudioSource brakeSource;
    public AudioSource crashAudioSource;
    public AudioClip crashClip;

    public AudioClip engineIdleClip;
    public AudioClip engineDriveClip;
    public AudioClip brakeClip;

    public float minPitch = 0.8f;
    public float maxPitch = 2.0f;

    [Header("Settings")]
    public float minCrashSpeed = 5f;

    [Header("Ghost Control")]
    public float ghostSteerForce = 10f; // how strong the pull is
    public float ghostAccelerationForce = 10f;
    private float ghostSteerTimer = 0f;
    private bool ghostActive = false;

    private float ghostSteerDirection = 1f;  // 1 = right, -1 = left
    private float ghostDirectionTimer = 0f;  // countdown to next direction flip
    private float ghostDirectionInterval = 0.5f; // how often it switches

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

        if (crashAudioSource != null)
        {
            AudioManager.Instance.RegisterSFXSource(crashAudioSource);
        }

        engineSource.clip = engineIdleClip;
        engineSource.loop = true;
        engineSource.Play();
    }


    void Update()
    {
        if (!playerDriving)
        {
            if (!ghostActive)
            {
                currentGear = 0; // Ensure we're in Park when not driving
            }
            
            return;
        }

        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        // Auto-shift to Drive when player accelerates from Park
        if (motorInput > 0.1f && currentGear == 0)
        {
            DriveGear();
        }

        // Gear shifting
        if (Input.GetKeyDown(KeyCode.E))
        {
            DriveGear();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ParkGear();
        }
    }

    void FixedUpdate()
    {
        if (ghostActive)
        {
            ghostSteerTimer -= Time.fixedDeltaTime;

            ghostDirectionTimer -= Time.fixedDeltaTime;

            // Remove brakes so ghost can accelerate freely
            rearLeft.brakeTorque = 0f;
            rearRight.brakeTorque = 0f;
            frontLeft.brakeTorque = 0f;
            frontRight.brakeTorque = 0f;

            if (ghostDirectionTimer <= 0f)
            {
                // Flip to opposite direction, with a small random interval
                ghostSteerDirection *= -1f;
                ghostDirectionInterval = Random.Range(0.3f, 0.8f); // randomize interval
                ghostDirectionTimer = ghostDirectionInterval;
            }

            float targetAngle = ghostSteerDirection * ghostSteerForce;
            frontLeft.steerAngle = targetAngle;
            frontRight.steerAngle = targetAngle;

            // Push forward
            rearLeft.motorTorque = ghostAccelerationForce;
            rearRight.motorTorque = ghostAccelerationForce;

            if (ghostSteerTimer <= 0f)
            {
                ghostActive = false;

                // Reset steering back to 0
                frontLeft.steerAngle = 0f;
                frontRight.steerAngle = 0f;

                // ✅ Reset motor torque so bus stops accelerating
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                frontLeft.motorTorque = 0f;
                frontRight.motorTorque = 0f;

                ghostSteerDirection = 1f;
                ghostDirectionTimer = 0f;
            }
            return;
        }

        // Apply Park constraints even when player isn't driving
        if (!playerDriving || currentGear == 0)
        {
            if (currentGear == 0 && !ghostActive)
            {
                ApplyParkBrakes();
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            if (!playerDriving && !ghostActive) return; // Return AFTER applying Park
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }

        if (!playerDriving) return;

        // Get current speed in km/h
        float signedSpeed = Vector3.Dot(rb.velocity, transform.forward) * 3.6f;
        float currentSpeed = Mathf.Abs(signedSpeed);

        bool isForward = motorInput > 0.1f;
        bool isReverse = motorInput < -0.1f;
        // Braking = pressing opposite direction to travel
        bool isBraking = (isForward && signedSpeed < -1f) || (isReverse && signedSpeed > 1f);

        float rampTarget = (isForward || isReverse) ? 1f : 0f;
        currentTorqueRamp = Mathf.MoveTowards(currentTorqueRamp, rampTarget, (1f / accelerationTime) * Time.fixedDeltaTime);

        // Boost torque during turns
        float turnBoost = 1f + (Mathf.Abs(steerInput) * 5f);

        // Boost torque during turns - MORE boost at higher speeds
        //float baseTurnBoost = 1f + (Mathf.Abs(steerInput) * 5f);

        // Extra boost if speed is dropping during turn
        /*float speedBoost = 1f;
        if (Mathf.Abs(steerInput) > 0.3f && currentSpeed < 30f)
        {
            speedBoost = 3f; // 50% extra power when turning slowly
        }*/

        // Braking detection (S key)
        /*bool isBraking = motorInput < -0.1f;
        float brakeSmoothing = 1f;
        float targetBrakeAmount = isBraking ? 1f : 0f;
        currentBrakeAmount = Mathf.MoveTowards(currentBrakeAmount, targetBrakeAmount, brakeSmoothing * Time.fixedDeltaTime);
        float currentBrakeTorque = brakeForce * currentBrakeAmount;*/

        // Reset engine brake when pressing W or S
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            engineBrakeAmount = 0f;
        }

        // Torque ramp: smoothly builds up from 0 when player presses accelerator
        //bool isAccelerating = motorInput > 0.1f;
        //float rampTarget = isAccelerating ? 1f : 0f;
        //currentTorqueRamp = Mathf.MoveTowards(currentTorqueRamp, rampTarget, (1f / accelerationTime) * Time.fixedDeltaTime);

        if (isBraking)
        {
            rearLeft.motorTorque = 0f;
            rearRight.motorTorque = 0f;
            frontLeft.brakeTorque = brakeForce * 0.004f;
            frontRight.brakeTorque = brakeForce * 0.004f;
            rearLeft.brakeTorque = 0f;
            rearRight.brakeTorque = 0f;
        }
        else if (isForward)
        {
            if (signedSpeed >= maxForwardSpeed)
            {
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                float capBrake = brakeForce * 0.1f;
                rearLeft.brakeTorque = capBrake;
                rearRight.brakeTorque = capBrake;
                frontLeft.brakeTorque = capBrake * 0.5f;
                frontRight.brakeTorque = capBrake * 0.5f;
            }
            else
            {
                float proximityFactor = Mathf.Clamp01(signedSpeed / maxForwardSpeed);
                float powerScale = 1f - (proximityFactor * proximityFactor * 0.6f);
                float torque = motorInput * motorForce * currentTorqueRamp * turnBoost * powerScale;
                rearLeft.motorTorque = torque;
                rearRight.motorTorque = torque;
                ClearBrakes();
            }
        }
        else if (isReverse)
        {
            if (-signedSpeed >= maxReverseSpeed)
            {
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                float capBrake = brakeForce * 0.1f;
                rearLeft.brakeTorque = capBrake;
                rearRight.brakeTorque = capBrake;
                frontLeft.brakeTorque = capBrake * 0.5f;
                frontRight.brakeTorque = capBrake * 0.5f;
            }
            else
            {
                float torque = motorInput * motorForce * currentTorqueRamp * turnBoost;
                rearLeft.motorTorque = torque;
                rearRight.motorTorque = torque;
                ClearBrakes();
            }
        }
        else
        {
            rearLeft.motorTorque = 0f;
            rearRight.motorTorque = 0f;
            ClearBrakes();
        }
        

        // At low speeds: full steering (60 degrees)
        // At high speeds: reduced steering (30 degrees)
        float speedFactor = Mathf.Clamp01(currentSpeed / 50f); // 0 at 0 km/h, 1 at 40+ km/h
        float dynamicSteerAngle = Mathf.Lerp(steerAngle, steerAngle * 0.8f, speedFactor);

        float currentSteerAngle = steerInput * dynamicSteerAngle;

        frontLeft.steerAngle = currentSteerAngle;
        frontRight.steerAngle = currentSteerAngle;

        // Adjust pitch based on speed
        float speedPercent = Mathf.Clamp01(rb.velocity.magnitude / maxForwardSpeed);
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
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void ApplyParkBrakes()
    {
        rearLeft.motorTorque = 0f;
        rearRight.motorTorque = 0f;
        rearLeft.brakeTorque = brakeForce * 10f;
        rearRight.brakeTorque = brakeForce * 10f;
        frontLeft.brakeTorque = brakeForce * 10f;
        frontRight.brakeTorque = brakeForce * 10f;
    }

    void CloseDoorsIfLeavingPark()
    {
        if (busDoors != null && busDoors.isOpen)
        {
            busDoors.CloseDoor();
            Debug.Log("Bus left Park, doors closed automatically.");
        }
    }

    // ── Gear Methods ──────────────────────────────────────────────────────────

    public void DriveGear()
    {
        if (currentGear == 1) return; // Already in Drive
        currentGear = 1;
        currentTorqueRamp = 0f; // Reset ramp so bus starts from 0
        CloseDoorsIfLeavingPark();
        Debug.Log("Shifted to DRIVE");
    }

    public void ParkGear()
    {
        currentGear = 0;
        currentTorqueRamp = 0f;
        Debug.Log("Shifted to PARK");
    }

    void ClearBrakes()
    {
        rearLeft.brakeTorque = 0f;
        rearRight.brakeTorque = 0f;
        frontLeft.brakeTorque = 0f;
        frontRight.brakeTorque = 0f;
    }

    // ── Ghost Event ───────────────────────────────────────────────────────────

    public void TriggerGhostEvent(float duration, float force, float accelForce)
    {
        ghostActive = true;
        ghostSteerTimer = duration;
        ghostSteerForce = force;
        ghostAccelerationForce = accelForce;

        // Start with a random direction
        ghostSteerDirection = Random.value > 0.5f ? 1f : -1f;
        ghostDirectionTimer = Random.Range(0.3f, 0.8f); // first flip time
    }

    public void ResetInputs()
    {
        motorInput = 0f;
        steerInput = 0f;
        currentTorqueRamp = 0f;

        // Immediately zero out wheels too
        rearLeft.motorTorque = 0f;
        rearRight.motorTorque = 0f;
        frontLeft.motorTorque = 0f;
        frontRight.motorTorque = 0f;

        frontLeft.steerAngle = 0f;
        frontRight.steerAngle = 0f;

        rearLeft.brakeTorque = 0f;
        rearRight.brakeTorque = 0f;
        frontLeft.brakeTorque = 0f;
        frontRight.brakeTorque = 0f;
    }

    // ── Tilt Clamp ────────────────────────────────────────────────────────────

    void ClampTilt()
    {
        Vector3 angles = rb.rotation.eulerAngles;

        float tiltX = NormalizeAngle(angles.x);
        float tiltZ = NormalizeAngle(angles.z);

        float maxTilt = 15f; // tweak this (10�20 good range)

        tiltX = Mathf.Clamp(tiltX, -maxTilt, maxTilt);
        tiltZ = Mathf.Clamp(tiltZ, -maxTilt, maxTilt);

        rb.rotation = Quaternion.Euler(tiltX, angles.y, tiltZ);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    // ── Bus Crash Audio ────────────────────────────────────────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Tree") || collision.transform.root.CompareTag("Tree"))
        {
            float speed = rb.velocity.magnitude * 3.6f;
            if (speed >= minCrashSpeed)
                PlayCrashAudio();
            Debug.Log("Collided with tree at " + speed.ToString("F1") + " km/h");
        }
    }

    void PlayCrashAudio()
    {
        if (crashAudioSource != null && crashClip != null)
        {
            crashAudioSource.PlayOneShot(crashClip);
        }
    }

    // ── GUI ────────────────────────────────────────────────────────────
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
        }
    }*/
}
