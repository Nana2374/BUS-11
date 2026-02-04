using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerController : MonoBehaviour, IInteractable
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
    private BusSeatManager seatManager;

    private enum PassengerState
    {
        Waiting,        // Standing at bus stop, waiting for bus
        WalkingToEntry,        // Walking towards bus entry point
        AtEntry,        // Standing at entry, waiting to be clicked
        WalkingToSeat,  // Walking to find a seat
        Seated          // Sitting in a seat
    }

    private PassengerState currentState = PassengerState.Waiting;
    private Transform targetSeat;

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

        seatManager = FindObjectOfType<BusSeatManager>();

        if (seatManager == null)
        {
            Debug.LogError("No BusSeatManager found in scene!");
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case PassengerState.Waiting:
                CheckForBus();
                break;

            case PassengerState.WalkingToEntry:
                CheckIfReachedEntry();
                break;

            case PassengerState.AtEntry:
                // Waiting for player to click
                break;

            case PassengerState.WalkingToSeat:
                CheckIfReachedSeat();
                break;

            case PassengerState.Seated:
                // Sitting down
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
            currentState = PassengerState.WalkingToEntry;
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
            ReachEntry();
        }
    }

    void ReachEntry()
    {
        currentState = PassengerState.AtEntry;
        agent.enabled = false; // Stop NavMesh agent

        // Snap passenger to entry point
        transform.position = entryPoint.position + new Vector3(0f, 0.8f, 0f);
        transform.rotation = entryPoint.rotation;

        // Optional: Make passenger a child of the bus so they move with it
        transform.SetParent(entryPoint.transform.parent);

        Debug.Log("Passenger boarded the bus!");
    }

    // Called when player clicks on the passenger (implements IInteractable)
    public void Interact()
    {
        if (currentState == PassengerState.AtEntry)
        {
            FindAndWalkToSeat();

            Debug.Log("Passenger clicked!");
        }
        else
        {
            Debug.Log("Passenger is not ready to sit yet!");
        }
    }

    void FindAndWalkToSeat()
    {
        if (seatManager == null)
        {
            Debug.LogError("SeatManager missing!");
            return;
        }

        targetSeat = seatManager.GetAvailableSeat();

        if (targetSeat == null)
        {
            Debug.Log("No available seats!");
            return;
        }

        seatManager.OccupySeat(targetSeat);

        currentState = PassengerState.WalkingToSeat;
        agent.enabled = true;
        agent.SetDestination(targetSeat.position);

        Debug.Log("Walking to seat: " + targetSeat.name);
    }

    void CheckIfReachedSeat()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            SitDown();
        }
    }

    void SitDown()
    {
        currentState = PassengerState.Seated;
        agent.enabled = false;

        // Snap to seat position and rotation
        transform.position = targetSeat.position;
        transform.rotation = targetSeat.rotation;

        Debug.Log("Passenger is now seated!");
    }
}
