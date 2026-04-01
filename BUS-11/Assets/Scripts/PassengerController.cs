using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class PassengerController : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public DialogueData passengerDialogue;

    [Header("UI")]
    public GameObject ticketUI; // Assign "Click on passenger to collect ticket"

    [Header("Passenger Type")]
    public bool isGhost = false; // Check this for the ghost passenger
    public bool isChild = false;
    public bool isElderly = false;

    //private Vector3 seatPositionOffset = ;

    [Header("Settings")]
    public float pickupRadius = 10f;       // How close the bus needs to be to the passenger
    public float busStopSpeed = 2f;        // Bus must be slower than this to count as "stopped"
    public float walkSpeed = 2f;           // Passenger walk speed

    [Header("References")]
    public Transform entryPoint;           // The PassengerEntry point on the bus (drag in Inspector)
    public Transform exitPoint;            // The PassengerExit point on the bus (drag in Inspector)

    [Header("Animation")]
    public Animator animator; // Add this

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 1f; // Check every second
    public float minMovementDistance = 0.3f; // Minimum movement to not be stuck
    private Vector3 lastPosition;
    private float stuckCheckTimer = 0f;

    private UnityEngine.AI.NavMeshAgent agent;
    private NavMeshSurface busNavMesh;

    public BusController busController;
    public BusDoors busDoors;
    public Rigidbody busRigidbody;
    public Transform busTransform;
    public BusSeatManager seatManager;
    public PassengerState CurrentState => currentState;

    public bool HasBeenPickedUp
    {
        get { return currentState != PassengerState.Waiting; }
    }

    public enum PassengerState
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

        if (ticketUI != null)
        {
            ticketUI.SetActive(false);
        }


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

    public void ShowUI()
    {
        if (ticketUI != null &&
         currentState == PassengerState.AtEntry &&
         !DialogueManager.Instance.IsDialogueActive())
        {
            ticketUI.SetActive(true);
        }
    }

    public void HideUI()
    {
        if (ticketUI != null)
        {
            ticketUI.SetActive(false);
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
                CheckIfStuck();
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
        if (isGhost)
        {
            transform.SetParent(entryPoint.transform.parent);

            FindAndWalkToSeat();
            Debug.Log("Ghost passenger skipping gesture and walking to seat.");
        }
        else
        {
            currentState = PassengerState.AtEntry;

            // Snap passenger to entry point
            transform.position = entryPoint.position;
            transform.rotation = entryPoint.rotation;

            agent.enabled = false; // Stop NavMesh agent

            // Optional: Make passenger a child of the bus so they move with it
            transform.SetParent(entryPoint.transform.parent);

            // Play idle animation
            SetAnimation(false, false);

            // ADD THIS
            if (passengerDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(passengerDialogue);
            }


            Debug.Log("Passenger boarded the bus!");
        }
    }

    // Called when player clicks on the passenger (implements IInteractable)
    public void Interact()
    {
        if (currentState == PassengerState.AtEntry)
        {
            HideUI();
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

        FindAndWalkToSeat();
    }

    void CheckIfStuck()
    {
        if (agent == null || !agent.enabled || targetSeat == null) return;

        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer >= stuckCheckInterval)
        {
            // Check if passenger has moved
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < minMovementDistance)
            {
                // Passenger is stuck! Switch to simple movement
                Debug.LogWarning($"{name} got stuck! Switching to simple movement.");

                // Disable NavMesh agent
                if (agent != null && agent.enabled)
                {
                    agent.enabled = false;
                }

                // Use simple walk instead of teleport
                StartCoroutine(WalkToSeatSimple());

                // Stop checking for stuck (we're now using simple movement)
                currentState = PassengerState.WalkingToSeat; // Keep state
                return; // Exit check
            }

            // Update last position and reset timer
            lastPosition = transform.position;
            stuckCheckTimer = 0f;
        }
    }

    IEnumerator WalkToSeatSimple()
    {
        float walkSpeed = 2f;

        while (Vector3.Distance(transform.position, targetSeat.position) > 0.3f)
        {
            // Move toward seat
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetSeat.position,
                walkSpeed * Time.deltaTime
            );

            // Face direction of movement
            Vector3 direction = (targetSeat.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    5f * Time.deltaTime
                );
            }

            yield return null;
        }
        SitDown();
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

        agent.enabled = true; // Enable NavMesh agent to walk to seat
        agent.SetDestination(targetSeat.position);

        // Play walking animation
        SetAnimation(true, false);

        seatManager.OccupySeat(targetSeat);

        currentState = PassengerState.WalkingToSeat;
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

        if (isChild)
        {
            // Apply child seat offset
            Vector3 localOffset = new Vector3(0f, 0.25f, -0.25f);
            transform.position = targetSeat.TransformPoint(localOffset);
            transform.rotation = targetSeat.rotation;
            Debug.Log("Child Offset applied.");
        }
        else if (isElderly)
        {
            // Apply elderly seat offset
            Vector3 localOffset = new Vector3(0f, 0.15f, -0.25f);
            transform.position = targetSeat.TransformPoint(localOffset);
            transform.rotation = targetSeat.rotation;
            Debug.Log("Elderly Offset applied.");
        }
        else
        {
            // Snap to seat position and rotation
            transform.position = targetSeat.position;
            transform.rotation = targetSeat.rotation;
        }

        // Play sitting animation
        SetAnimation(false, true);

        Debug.Log("Passenger is now seated!");
    }

    void CheckForStop()
    {
        if (busTransform == null || busRigidbody == null || exitPoint == null) return;

        float distanceToStop = Vector3.Distance(busTransform.position, exitPoint.position);
        float busSpeed = busRigidbody.velocity.magnitude * 3.6f; // Convert to km/h

        // Check if bus is close enough AND slow enough (stopped) AND in Park gear
        if (distanceToStop <= pickupRadius &&
            busSpeed <= busStopSpeed &&
            busController.currentGear == 0 && // Must be in Park (gear 0)
            busDoors.isOpen == true) //Bus doors must be open
        {
            currentState = PassengerState.WalkingToExit;

            // Unparent from bus so they can walk away
            transform.SetParent(null);
            agent.enabled = true;
            agent.SetDestination(exitPoint.position);

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
