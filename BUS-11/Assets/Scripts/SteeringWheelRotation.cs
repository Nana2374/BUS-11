using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheelRotation : MonoBehaviour
{
    public BusController busController; // Reference to the bus controller script

    [Header("Settings")]
    public float maxRotationAngle = 450f;  // How many degrees the wheel can turn (1.25 full rotations)
    public float rotationSpeed = 1f;       // How fast the wheel rotates

    private float currentRotation = 0f;
    private float targetRotation = 0f;
    private Quaternion initialRotation;

    void Start()
    {
        if (busController == null)
        {
            GameObject bus = GameObject.FindGameObjectWithTag("Bus");
            if (bus != null)
            {
                busController = bus.GetComponent<BusController>();
            }
        }

        // Store the initial rotation of the steering wheel
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        if (busController != null && busController.playerDriving == true)
        {
            // Get steering input
            float steerInput = Input.GetAxis("Horizontal"); // -1 to 1

            // Calculate target rotation
            targetRotation = steerInput * maxRotationAngle;

            // Smoothly rotate towards target
            currentRotation = Mathf.Lerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Apply rotation relative to initial rotation
            Quaternion steeringRotation = Quaternion.Euler(0, currentRotation, 0);
            transform.localRotation = initialRotation * steeringRotation;
        }
    }
}
