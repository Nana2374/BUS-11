using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearPosition : MonoBehaviour
{
    [Header("Gear Positions")]
    public Transform parkPosition;      // P
    public Transform reversePosition;   // R
    public Transform gear1Position;     // 1
    public Transform gear2Position;     // 2
    public Transform gear3Position;     // 3

    [Header("Settings")]
    public float moveSpeed = 10f;       // How fast gear stick moves
    public BusController busController; // Reference to bus

    private Quaternion initialRotation;
    private Vector3 targetPosition;

    void Start()
    {
        initialRotation = transform.localRotation;

        if (busController == null)
        {
            busController = FindObjectOfType<BusController>();
        }

        // Start at park position
        if (parkPosition != null)
        {
            transform.position = parkPosition.position;
        }
    }

    void Update()
    {
        if (busController == null) return;

        // Get target position based on current gear
        targetPosition = GetGearPosition(busController.currentGear);

        // Smoothly move to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Keep original rotation
        transform.localRotation = initialRotation;
    }

    Vector3 GetGearPosition(int gear)
    {
        switch (gear)
        {
            case -1: return reversePosition != null ? reversePosition.position : transform.position;
            case 0: return parkPosition != null ? parkPosition.position : transform.position;
            case 1: return gear1Position != null ? gear1Position.position : transform.position;
            case 2: return gear2Position != null ? gear2Position.position : transform.position;
            case 3: return gear3Position != null ? gear3Position.position : transform.position;
            default: return transform.position;
        }
    }
}
