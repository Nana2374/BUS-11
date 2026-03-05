using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float detectionRadius = 15f;      // How far the chicken can detect the bus
    public float panicRadius = 8f;           // Distance when chicken starts running

    [Header("Crossing Settings")]
    public Transform endPosition;

    private enum ChickenState
    {
        Idle,           // Standing still
        Walking,        // Walking across road
        Running,        // Running (panicking)
        Dead,
        CrossedRoad     // Finished crossing
    }

    private ChickenState currentState = ChickenState.Idle;
    private Transform busTransform;
    private Rigidbody busRigidbody;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float currentSpeed;
    private Animator animator; // Optional: for animations

    void Start()
    {
        // Find the bus
        GameObject bus = GameObject.FindGameObjectWithTag("Bus");
        if (bus != null)
        {
            busTransform = bus.transform;
            busRigidbody = bus.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("Bus not found! Make sure bus has 'Bus' tag.");
        }

        if (endPosition != null)
        {
            targetPosition = endPosition.position;
        }
        else
        {
            Debug.LogError("No end position set for chicken!");
        }

        // Store starting position
        startPosition = transform.position;

        // Try to get animator if exists
        animator = GetComponent<Animator>();

        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (busTransform == null) return;

        switch (currentState)
        {
            case ChickenState.Idle:
                CheckForBus();
                break;

            case ChickenState.Walking:
                CrossRoad();
                CheckForPanic();
                break;

            case ChickenState.Running:
                CrossRoad();
                break;

            case ChickenState.Dead:
                // Do nothing - chicken is frozen
                break;

            case ChickenState.CrossedRoad:
                // Chicken made it! Do nothing
                break;
        }
    }

    void CheckForBus()
    {
        float distanceToBus = Vector3.Distance(transform.position, busTransform.position);

        // Start crossing when bus is near
        if (distanceToBus <= detectionRadius)
        {
            currentState = ChickenState.Walking;
            currentSpeed = walkSpeed;

            // Face the end position
            Vector3 directionToEnd = (targetPosition - transform.position).normalized;
            if (directionToEnd != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToEnd);
            }

            Debug.Log("Chicken detected bus! Starting to cross road.");

            // Optional: Play walk animation
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
    }

    void CheckForPanic()
    {
        float distanceToBus = Vector3.Distance(transform.position, busTransform.position);

        // Panic and run if bus gets too close
        if (distanceToBus <= panicRadius)
        {
            currentState = ChickenState.Running;
            currentSpeed = runSpeed;

            Debug.Log("Chicken panicking! Running across road!");

            // Optional: Play run animation
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", true);
            }
        }
    }

    void CrossRoad()
    {
        // Move towards target position
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentSpeed * Time.deltaTime
        );

        // Keep facing the target
        Vector3 directionToEnd = (targetPosition - transform.position).normalized;
        if (directionToEnd != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(directionToEnd),
                10f * Time.deltaTime
            );
        }

        // Check if reached other side
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget < 0.1f)
        {
            currentState = ChickenState.CrossedRoad;

            Debug.Log("Chicken safely crossed the road!");

            // Optional: Stop animations
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isIdle", true);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if hit by bus
        if (collision.gameObject.CompareTag("Bus"))
        {
            Debug.Log("Chicken got hit!");

            // Freeze chicken at current position
            currentState = ChickenState.Dead;

            // Stop all animations
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.enabled = false; // Completely freeze animator
            }

            // Optional: Tip the chicken over
            transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, 0);
        }
    }

    // Optional: Visualize detection radius in editor
    void OnDrawGizmosSelected()
    {
        // Detection radius (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Panic radius (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, panicRadius);

        // Target position (green)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
