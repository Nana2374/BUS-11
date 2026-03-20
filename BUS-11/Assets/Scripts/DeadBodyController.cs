using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBodyController : MonoBehaviour
{
    [Header("Detection Settings")]
    public BoxCollider detectionTrigger;     // Assign a box collider trigger
    public float activationSpeed = 1f;       // Bus must be going faster than this (km/h)

    [Header("Flight Settings")]
    public Transform deadBody;
    public Rigidbody bodyRigidbody;
    public float dropDistance = 15f;
    //public Transform endPosition;            // Where the body flies to
    //public float launchForce = 10f;          // How hard to launch upward/forward
    //public float launchAngle = 45f;          // Launch angle (degrees)

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
    private bool hasLaunched = false;
    private bool busInTrigger = false;

    void Start()
    {
        if (bodyRigidbody == null)
        {
            bodyRigidbody = deadBody.GetComponent<Rigidbody>();
        }

        // Start frozen (kinematic)
        bodyRigidbody.isKinematic = true;
        bodyRigidbody.useGravity = false;

        deadBody.gameObject.SetActive(false);
    }

    void Update()
    {
        if (hasLaunched) return;

        if (currentState == BodyState.Lying && busInTrigger)
        {
            CheckForBus();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider OR its parent has the "Bus" tag
        if (other.CompareTag("Bus") || other.transform.root.CompareTag("Bus"))
        {
            busInTrigger = true;
            busRigidbody = other.GetComponent<Rigidbody>();
            Debug.Log("Bus entered detection zone!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if bus left trigger
        if (other.CompareTag("Bus") || other.transform.root.CompareTag("Bus"))
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
        deadBody.gameObject.SetActive(true);

        hasLaunched = true;
        currentState = BodyState.Flying;

        // UNPARENT from bus first!
        deadBody.SetParent(null);

        // Position in front of the bus
        Vector3 busForward = busRigidbody.transform.forward;
        float dropDistance = 15f; // How far in front (adjust in Inspector)

        Vector3 dropPosition = deadBody.position + (busForward * dropDistance);
        deadBody.position = dropPosition;

        // Enable physics
        bodyRigidbody.isKinematic = false;
        bodyRigidbody.useGravity = true;

        // Start landing timer
        Invoke("MarkAsLanded", 3f); // Call MarkAsLanded after 1 second

        // Calculate launch direction
        /*Vector3 directionToEnd = (endPosition.position - deadBody.position).normalized;

        // Calculate launch velocity using projectile motion
        float distance = Vector3.Distance(deadBody.position, endPosition.position);
        float angleRad = launchAngle * Mathf.Deg2Rad;

        // Calculate required velocity for ballistic trajectory
        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocity = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * angleRad));

        // Apply launch force
        Vector3 launchVelocity = directionToEnd * velocity * Mathf.Cos(angleRad);
        launchVelocity.y = velocity * Mathf.Sin(angleRad);

        bodyRigidbody.velocity = launchVelocity;*/

        Debug.Log("Dead body dropped!");
    }

    void MarkAsLanded()
    {
        currentState = BodyState.Landed;
        Debug.Log("Dead body landed!");

        bodyRigidbody.isKinematic = true;

        if (destroyOnLand)
            {
                Destroy(gameObject, destroyDelay);
                Debug.Log("Dead body will be destroyed in " + destroyDelay + " seconds.");
            }
    }

    /*void OnDrawGizmos()
    {
        // End position (red)
        if (endPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPosition.position, 1f);
            Gizmos.DrawLine(deadBody.position, endPosition.position);
        }
    }*/
}
