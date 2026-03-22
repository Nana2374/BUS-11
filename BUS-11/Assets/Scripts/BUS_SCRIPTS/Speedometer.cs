using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [Header("References")]
    public BusController busController;

    [Header("Speed Range")]
    public float minSpeed = 0f;      // km/h at minimum angle
    public float maxSpeed = 60f;    // km/h at maximum angle

    [Header("Rotation Angles (Z-axis)")]
    public float minAngle = 85f;      // Angle at 0 km/h
    public float maxAngle = -25f;    // Angle at max speed

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f; // How fast needle follows speed changes

    private float initialX;
    private float initialY;

    void Start()
    {
        // Auto-find BusController if not assigned
        if (busController == null)
        {
            busController = FindObjectOfType<BusController>();
        }

        // Store initial X and Y rotation (keep these fixed)
        Vector3 currentRotation = transform.localEulerAngles;
        initialX = currentRotation.x;
        initialY = currentRotation.y;

        //Debug.Log($"Speedometer initialized - X: {initialX}, Y: {initialY}");

        // Set to minimum speed position at start
        transform.localRotation = Quaternion.Euler(initialX, initialY, minAngle);
    }

    void Update()
    {
        if (busController == null) return;

        // Get current speed from bus (in km/h)
        float currentSpeed = busController.rb.velocity.magnitude * 3.6f;

        // Clamp speed to min/max range
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);

        // Map speed to rotation angle
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, currentSpeed / maxSpeed);

        // Target rotation - ONLY change Z, keep X and Y from initial
        Quaternion targetRotation = Quaternion.Euler(initialX, initialY, targetAngle);

        // Smoothly rotate to target
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
