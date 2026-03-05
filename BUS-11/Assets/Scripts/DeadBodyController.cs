using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBodyController : MonoBehaviour
{
    [Header("Detection Settings")]
    public BoxCollider detectionTrigger;     // Assign a box collider trigger
    public float activationSpeed = 5f;       // Bus must be going faster than this (km/h)

    [Header("Flight Settings")]
    public Transform endPosition;            // Where the body flies to
    public float launchForce = 10f;          // How hard to launch upward/forward
    public float launchAngle = 45f;          // Launch angle (degrees)

    [Header("Auto-Destroy")]
    public bool destroyOnLand = true;        // Destroy after reaching end position
    public float destroyDelay = 2f;          // Seconds after landing to destroy

    private enum BodyState
    {
        Lying,          // Just lying on ground
        Flying,         // Launched in air
        Landed          // Reached end position
    }

    private BodyState currentState = BodyState.Lying;
    private Rigidbody busRigidbody;
    private Rigidbody bodyRigidbody;
    private bool hasLaunched = false;
    private bool busInTrigger = false;

    void Start()
    {
        // Get or add rigidbody to body
        bodyRigidbody = GetComponent<Rigidbody>();
        if (bodyRigidbody == null)
        {
            bodyRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // Start frozen (kinematic)
        bodyRigidbody.isKinematic = true;
        bodyRigidbody.useGravity = false;

        // Setup trigger if not assigned
        if (detectionTrigger == null)
        {
            // Create trigger as child object
            GameObject triggerObj = new GameObject("DetectionTrigger");
            triggerObj.transform.SetParent(transform);
            triggerObj.transform.localPosition = Vector3.zero;

            detectionTrigger = triggerObj.AddComponent<BoxCollider>();
            detectionTrigger.isTrigger = true;
            detectionTrigger.size = new Vector3(10f, 3f, 10f); // Default size

            Debug.Log("Auto-created detection trigger. Adjust size in Inspector.");
        }
        else
        {
            // Make sure it's set as trigger
            detectionTrigger.isTrigger = true;
        }

        // Create end position if not assigned
        if (endPosition == null)
        {
            GameObject endPosObj = new GameObject("DeadBodyEndPosition");
            endPosition = endPosObj.transform;
            endPosition.position = transform.position + transform.forward * 20f + Vector3.up * 2f;
        }
    }

    void Update()
    {
        if (hasLaunched) return;

        if (currentState == BodyState.Lying && busInTrigger)
        {
            CheckForBus();
        }
        else if (currentState == BodyState.Flying)
        {
            CheckIfLanded();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if bus entered trigger
        if (other.CompareTag("Bus"))
        {
            busInTrigger = true;
            busRigidbody = other.GetComponent<Rigidbody>();
            Debug.Log("Bus entered detection zone!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if bus left trigger
        if (other.CompareTag("Bus"))
        {
            busInTrigger = false;
            Debug.Log("Bus left detection zone.");
        }
    }

    void CheckForBus()
    {
        if (busRigidbody == null) return;

        float busSpeed = busRigidbody.velocity.magnitude * 3.6f; // Convert to km/h

        // Launch if bus is in trigger and moving fast enough
        if (busSpeed >= activationSpeed)
        {
            LaunchBody();
        }
    }

    void LaunchBody()
    {
        hasLaunched = true;
        currentState = BodyState.Flying;

        // Enable physics
        bodyRigidbody.isKinematic = false;
        bodyRigidbody.useGravity = true;

        // Calculate launch direction
        Vector3 directionToEnd = (endPosition.position - transform.position).normalized;

        // Calculate launch velocity using projectile motion
        float distance = Vector3.Distance(transform.position, endPosition.position);
        float angleRad = launchAngle * Mathf.Deg2Rad;

        // Calculate required velocity for ballistic trajectory
        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocity = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * angleRad));

        // Apply launch force
        Vector3 launchVelocity = directionToEnd * velocity * Mathf.Cos(angleRad);
        launchVelocity.y = velocity * Mathf.Sin(angleRad);

        bodyRigidbody.velocity = launchVelocity;
    }

    void CheckIfLanded()
    {
        // Check if close to end position or stopped moving
        float distanceToEnd = Vector3.Distance(transform.position, endPosition.position);
        bool isStopped = bodyRigidbody.velocity.magnitude < 0.5f;

        if ((distanceToEnd < 2f || isStopped) && currentState == BodyState.Flying)
        {
            currentState = BodyState.Landed;
            Debug.Log("Dead body landed!");

            if (destroyOnLand)
            {
                Destroy(gameObject, destroyDelay);
                Debug.Log("Dead body will be destroyed in " + destroyDelay + " seconds.");
            }
        }
    }

    void OnDrawGizmos()
    {
        // End position (red)
        if (endPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPosition.position, 1f);
            Gizmos.DrawLine(transform.position, endPosition.position);
        }
    }
}
