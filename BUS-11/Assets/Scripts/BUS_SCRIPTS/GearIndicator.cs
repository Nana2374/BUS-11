using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearIndicator : MonoBehaviour
{
    [Header("References")]
    public BusController busController;

    [Header("Gear Rotation Angles (X-axis)")]
    public float parkAngle = -67f;      // Gear P
    public float reverseAngle = -138f; // Gear R
    public float gear1Angle = 5f;    // Gear 1
    public float gear2Angle = 78f;    // Gear 2
    public float gear3Angle = 150f;    // Gear 3

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f; // How fast it rotates to new position

    private float initialY;
    private float initialZ;

    void Start()
    {
        // Auto-find BusController if not assigned
        if (busController == null)
        {
            busController = FindObjectOfType<BusController>();
        }

        // Store initial Y and Z rotation (keep these fixed)
        Vector3 currentRotation = transform.localEulerAngles;
        initialY = currentRotation.y;
        initialZ = currentRotation.z;

        // Set to Park position immediately
        transform.localRotation = Quaternion.Euler(parkAngle, initialY, initialZ);
    }

    void Update()
    {
        if (busController == null) return;

        // Get current gear from bus controller
        int currentGear = busController.currentGear;

        // Determine target angle based on gear
        float targetAngle = GetAngleForGear(currentGear);

        // Target rotation - ONLY change X, keep Y and Z from initial
        Quaternion targetRotation = Quaternion.Euler(targetAngle, initialY, initialZ);

        // Smoothly rotate to target
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
            );
    }

    float GetAngleForGear(int gear)
    {
        switch (gear)
        {
            case -1: return reverseAngle;  // Reverse
            case 0: return parkAngle;     // Park
            case 1: return gear1Angle;    // Gear 1
            case 2: return gear2Angle;    // Gear 2
            case 3: return gear3Angle;    // Gear 3
            default: return parkAngle;
        }
    }
}
