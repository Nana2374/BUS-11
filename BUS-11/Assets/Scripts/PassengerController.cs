using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class PassengerController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public float pickupRadius = 10f;       // How close the bus needs to be to the passenger
    public float busStopSpeed = 2f;        // Bus must be slower than this to count as "stopped"
    public float walkSpeed = 2f;           // Passenger walk speed

    [Header("References")]
    public Transform entryPoint;           // The PassengerEntry point on the bus (drag in Inspector)
    public Transform exitPoint;            // The PassengerExit point on the bus (drag in Inspector)

    [Header("Animation")]
    public Animator animator; // Add this

    private UnityEngine.AI.NavMeshAgent agent;
    private NavMeshSurface busNavMesh;
    public BusController busController;
    public BusDoors busDoors;
    public Rigidbody busRigidbody;
    public Transform busTransform;
    public BusSeatManager seatManager;

    private enum PassengerState
    {
        Waiting,        // Standing at bus stop, waiting for bus
        WalkingToEntry,        // Walking towards bus entry point
        AtEntry,        // Standing at entry, waiting to be clicked
        WalkingToSeat,  // Walking to find a seat
        Seated,          // Sitting in a seat
        WalkingToExit,  // Walking towards bus exit point
        Exited           // Exited the bus and walking away

    }

    private PassengerState currentState = PassengerState.Waiting;
    private Transform targetSeat;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = walkSpeed;

        // Get animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Find the bus and its components
        GameObject bus = GameObject.FindGameObjectWithTag("Bus");
        if (bus != null)
        {
            busController = bus.GetComponent<BusController>();
            busRigidbody = bus.GetComponent<Rigidbody>();
            busTransform = bus.transform;
            busNavMesh = bus.GetComponentInChildren<NavMeshSurface>();

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

        // Start with idle animation
        SetAnimation(false, false);
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
                CheckForStop();
                break;

            case PassengerState.WalkingToExit:
                CheckIfReachedExit();
                break;

            case PassengerState.Exited:
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
            busController.currentGear == 0 && // Must be in Park (gear 0)
            busDoors.isOpen == true) //Bus doors must be open
        {
            // Play walking animation
            SetAnimation(true, false);

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
        transform.position = entryPoint.position;
        transform.rotation = entryPoint.rotation;

        // Optional: Make passenger a child of the bus so they move with it
        transform.SetParent(entryPoint.transform.parent);

        // Play idle animation
        SetAnimation(false, false);

        Debug.Log("Passenger boarded the bus!");
    }

    // Called when player clicks on the passenger (implements IInteractable)
    public void Interact()
    {
        if (currentState == PassengerState.AtEntry)
        {
            // Start gesture animation, then walk to seat after it finishes
            StartCoroutine(GestureThenWalkToSeat());
        }
    }

    IEnumerator GestureThenWalkToSeat()
    {
        // Play gesture animation
        if (animator != null)
        {
            animator.SetTrigger("Gesture");
        }

        // Wait for gesture animation to finish
        yield return new WaitForSeconds(2.5f); // Change this to your gesture duration

        // Re-enable agent for walking on bus
        agent.enabled = true;

        FindAndWalkToSeat();
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
        

        // Play walking animation
        SetAnimation(true, false);

        seatManager.OccupySeat(targetSeat);

        currentState = PassengerState.WalkingToSeat;
       
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

        // Play sitting animation
        SetAnimation(false, true);

        Debug.Log("Passenger is now seated!");
    }

    void CheckForStop()
    {
        if (busTransform == null || busRigidbody == null) return;

        float distanceToStop = Vector3.Distance(busTransform.position, exitPoint.position);
        float busSpeed = busRigidbody.velocity.magnitude * 3.6f; // Convert to km/h

        // Check if bus is close enough AND slow enough (stopped) AND in Park gear
        if (distanceToStop <= pickupRadius &&
            busSpeed <= busStopSpeed &&
            busController.currentGear == 0 && // Must be in Park (gear 0)
            busDoors.isOpen == true) //Bus doors must be open
        {
            currentState = PassengerState.WalkingToExit;
            agent.enabled = true;
            agent.SetDestination(exitPoint.position);

            // Unparent from bus so they can walk away
            transform.SetParent(null);

            // Play walking animation
            SetAnimation(true, false);

            Debug.Log(name + " alighting.");
        }

    }

    void CheckIfReachedExit()
    {
        if (agent.pathPending) return;

        // Check if reached the entry point
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            ReachExit();
        }
    }

    void ReachExit()
    {
        currentState = PassengerState.Exited;
        agent.enabled = false; // Stop NavMesh agent

        // Snap passenger to exit point
        transform.position = exitPoint.position;
        transform.rotation = exitPoint.rotation;

        // Play idle animation
        SetAnimation(false, false);

        Debug.Log("Passenger alighted the bus!");
    }

    // Helper function to set animations
    void SetAnimation(bool walking, bool sitting)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isSitting", sitting);
    }
}
