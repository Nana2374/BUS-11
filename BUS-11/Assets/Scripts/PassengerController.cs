using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerController : MonoBehaviour
{
    [Header("Settings")]
    public float pickupRadius = 10f;       // How close the bus needs to be to the passenger
    public float busStopSpeed = 2f;        // Bus must be slower than this to count as "stopped"
    public float walkSpeed = 2f;           // Passenger walk speed

    [Header("References")]
    public Transform entryPoint;           // The PassengerEntry point on the bus (drag in Inspector)

    private UnityEngine.AI.NavMeshAgent agent;
    private BusController busController;
    private Rigidbody busRigidbody;
    private Transform busTransform;

    private enum PassengerState
    {
        Waiting,        // Standing at bus stop, waiting for bus
        Walking,        // Walking towards bus entry point
        Boarded         // Inside the bus
    }

    private PassengerState currentState = PassengerState.Waiting;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = walkSpeed;

        // Find the bus and its components
        GameObject bus = GameObject.FindGameObjectWithTag("Bus");
        if (bus != null)
        {
            busController = bus.GetComponent<BusController>();
            busRigidbody = bus.GetComponent<Rigidbody>();
            busTransform = bus.transform;

            // Find the entry point on the bus if not manually assigned
            if (entryPoint == null)
            {
                entryPoint = busTransform.Find("PassengerEntryPoint");
            }
        }
        else
        {
            Debug.LogError("Bus not found! Make sure your bus GameObject is named 'Bus'.");
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case PassengerState.Waiting:
                CheckForBus();
                break;

            case PassengerState.Walking:
                CheckIfReachedEntry();
                break;

            case PassengerState.Boarded:
                // Do nothing, passenger is on the bus
                break;
        }
    }

    void CheckForBus()
    {
        if (busTransform == null || busRigidbody == null) return;

        float distanceToBus = Vector3.Distance(transform.position, busTransform.position);
        float busSpeed = busRigidbody.velocity.magnitude * 3.6f; // Convert to km/h

        // Check if bus is close enough AND slow enough (stopped) AND in Park gear
        if (distanceToBus <= pickupRadius &&
            busSpeed <= busStopSpeed &&
            busController.currentGear == 0) // Must be in Park (gear 0)
        {
            currentState = PassengerState.Walking;
            agent.SetDestination(entryPoint.position);
            Debug.Log("Bus is in Park! Walking to entry point.");
        }
    }

    void CheckIfReachedEntry()
    {
        // Wait for NavMesh to calculate path
        if (agent.pathPending) return;

        // Check if reached the entry point
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            Board();
        }
    }

    void Board()
    {
        currentState = PassengerState.Boarded;
        agent.enabled = false; // Stop NavMesh agent

        // Snap passenger to entry point
        transform.position = entryPoint.position + new Vector3(0f, 0.8f, 0f);
        transform.rotation = entryPoint.rotation;

        // Optional: Make passenger a child of the bus so they move with it
        transform.SetParent(entryPoint.transform.parent);

        Debug.Log("Passenger boarded the bus!");
    }
}
