using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class PassengerController : MonoBehaviour, IInteractable
{
    //Passenger Queue System
    private static List<PassengerController> waitingPassengers = new List<PassengerController>();
    private static PassengerController activeBoardingPassenger = null;

    private bool usingSimpleMovement = false;
    private bool hasJoinedQueue = false;

    [Header("Priority")]
    public int boardingPriority = 0; // Lower = boards first

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

    private bool gestureStarted = false;

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

    private static PassengerController GetNextPassengerByPriority()
    {
        if (waitingPassengers.Count == 0)
            return null;

        PassengerController bestPassenger = null;
        int bestPriority = int.MaxValue;

        foreach (PassengerController passenger in waitingPassengers)
        {
            if (passenger == null)
                continue;

            if (passenger.currentState != PassengerState.Waiting)
                continue;

            if (passenger.boardingPriority < bestPriority)
            {
                bestPriority = passenger.boardingPriority;
                bestPassenger = passenger;
            }
        }

        return bestPassenger;
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
        float busSpeed = busRigidbody.velocity.magnitude * 3.6f;

        bool busReady =
            distanceToBus <= pickupRadius &&
            busSpeed <= busStopSpeed &&
            busController.currentGear == 0 &&
            busDoors.isOpen == true;

        if (!busReady) return;

        // Add passenger to waiting list only once
        if (!hasJoinedQueue)
        {
            waitingPassengers.Add(this);
            hasJoinedQueue = true;
            Debug.Log(name + " joined waiting list.");
        }

        // If nobody is boarding, pick the closest waiting passenger to the door
        if (activeBoardingPassenger == null)
        {
            activeBoardingPassenger = GetNextPassengerByPriority();

            if (activeBoardingPassenger != null)
            {
                waitingPassengers.Remove(activeBoardingPassenger);
                Debug.Log(activeBoardingPassenger.name + " chosen as closest to board.");
            }
        }

        // Only chosen passenger may board
        if (activeBoardingPassenger == this && currentState == PassengerState.Waiting)
        {
            SetAnimation(true, false);
            currentState = PassengerState.WalkingToEntry;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(entryPoint.position);
            }

            Debug.Log(name + " is now boarding.");
        }
    }

    void ReleaseNextPassenger()
    {
        if (activeBoardingPassenger == this)
        {
            activeBoardingPassenger = GetNextPassengerByPriority();

            if (activeBoardingPassenger != null)
            {
                waitingPassengers.Remove(activeBoardingPassenger);
                Debug.Log(activeBoardingPassenger.name + " is next to board (closest to door).");
            }
        }
    }

    void OnDisable()
    {
        waitingPassengers.Remove(this);

        if (activeBoardingPassenger == this)
        {
            activeBoardingPassenger = null;
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
        if (currentState == PassengerState.AtEntry && !gestureStarted)
        {
            gestureStarted = true;
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
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || targetSeat == null || usingSimpleMovement)
            return;

        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < minMovementDistance)
            {
                Debug.LogWarning($"{name} got stuck! Switching to simple movement.");

                usingSimpleMovement = true;

                if (agent.enabled)
                {
                    agent.ResetPath();
                    agent.enabled = false;
                }

                StartCoroutine(WalkToSeatSimple());
                return;
            }

            lastPosition = transform.position;
            stuckCheckTimer = 0f;
        }
    }

    IEnumerator WalkToSeatSimple()
    {
        usingSimpleMovement = true;

        float simpleWalkSpeed = 2f;

        while (targetSeat != null && Vector3.Distance(transform.position, targetSeat.position) > 0.3f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetSeat.position,
                simpleWalkSpeed * Time.deltaTime
            );

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

        usingSimpleMovement = false;
        SitDown();
    }

    void SnapToSeatIfBusMoving()
    {
        if (targetSeat == null || busRigidbody == null) return;

        float busSpeed = busRigidbody.velocity.magnitude * 3.6f;

        // If bus has started moving while passenger is still walking to seat, snap them immediately
        if (busSpeed > 0.5f)
        {
            // Stop NavMesh
            if (agent != null && agent.enabled)
            {
                agent.ResetPath();
                agent.enabled = false;
            }

            WalkToSeatSimple();
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

        usingSimpleMovement = false;
        stuckCheckTimer = 0f;
        lastPosition = transform.position;

        if (agent != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(targetSeat.position);
            }
            else
            {
                Debug.LogWarning($"{name} is not on NavMesh. Switching to simple movement.");
                usingSimpleMovement = true;
                StartCoroutine(WalkToSeatSimple());
                return;
            }
        }

        SetAnimation(true, false);

        seatManager.OccupySeat(targetSeat);

        currentState = PassengerState.WalkingToSeat;

        ReleaseNextPassenger();

    }

    void CheckIfReachedSeat()
    {
        if (usingSimpleMovement) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            SitDown();
        }
    }

    void SitDown()
    {
        currentState = PassengerState.Seated;
        usingSimpleMovement = false;

        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

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
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (agent.pathPending) return;

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
        // Destroy after short delay
        Destroy(gameObject, 1.5f);
    }

    // Helper function to set animations
    void SetAnimation(bool walking, bool sitting)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isSitting", sitting);
    }
}
