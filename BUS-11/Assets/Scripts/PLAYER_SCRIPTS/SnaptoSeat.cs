using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnaptoSeat : MonoBehaviour
{
    public Transform driverSeat;
    public Transform exitPoint;
    public BusController bus;
    public Transform busDriver;

    [Header("Camera Setup")]
    public Camera mainCamera; // Use Camera component instead of Transform
    public CameraShake cameraShake; // Add reference to camera shake script

    [Header("Camera Positions")]
    public Vector3 seatedCameraLocalPos = new Vector3(0f, 0.6f, 0.1f); // Seated position (local to Player)

    [Header("Animation")]
    public Animator animator;

    private PlayerMovement playerMovement;
    private bool playerInRange;
    public bool isSeated;

    private Transform originalMeshParent;
    private Transform cameraTransform;
    private Vector3 originalCameraLocalPos; // Store LOCAL position

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        // Store original parent
        if (busDriver != null)
        {
            originalMeshParent = busDriver.parent;
        }

        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
            // Store original LOCAL position (relative to Player)
            originalCameraLocalPos = cameraTransform.localPosition;

            Debug.Log($"Camera original local position: {originalCameraLocalPos}");
        }
        else
        {
            Debug.LogError("Camera not found!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!isSeated && playerInRange)
                SnapPlayerToSeat();
            else if (isSeated)
                ExitSeat();
        }

        if (isSeated && driverSeat != null)
        {
            busDriver.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void LateUpdate()
    {
        // Keep mesh position synced with player when seated but unparented
        if (isSeated && busDriver != null)
        {
            busDriver.position = driverSeat.position;
            busDriver.rotation = driverSeat.rotation; 
        }

        // Force camera position when seated
        if (isSeated && cameraTransform != null)
        {
            cameraTransform.localPosition = seatedCameraLocalPos;
        }
    }

    void SnapPlayerToSeat()
    {
        // ADD THIS CHECK AT THE START
    if (!playerInRange)
    {
        Debug.Log("Cannot enter seat - not in range!");
        return;
    }

        if (driverSeat == null)
        {
            GameObject seatObj = GameObject.FindGameObjectWithTag("DriverSeat");
            if (seatObj != null)
                driverSeat = seatObj.transform;
        }

        if (driverSeat == null) return;

        playerMovement.enabled = false;

        // Snap to seat
        transform.position = driverSeat.position + new Vector3(0.1f, 0.45f, 0.45f);
        //transform.rotation = driverSeat.rotation;
        transform.SetParent(driverSeat);

        // UNPARENT the mesh so it doesn't rotate with Player
        if (busDriver != null)
        {
            busDriver.SetParent(driverSeat); // Parent to seat instead of player
            busDriver.position = Vector3.zero; // Reset local position to seat
            busDriver.rotation = Quaternion.identity; // Face forward
        }

        // Auto-find mesh if not assigned
        if (busDriver == null)
        {
            busDriver = transform.Find("BusDriver_Idle"); // Use exact name from hierarchy
        }

        // Change camera to seated position (LOCAL position - relative to Player)
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = seatedCameraLocalPos;
            Debug.Log($"Camera seated at local position: {cameraTransform.localPosition}");
        }

        // DISABLE CameraShake when seated
        if (cameraShake != null)
        {
            cameraShake.enabled = false;
        }

        // Rigidbody fix (if needed)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Enable bus driving
        if (bus != null)
            bus.playerDriving = true;

        isSeated = true;
        Debug.Log("Entered seat");

        // Play sitting animation
        SetAnimation(false, true);
    }

    void ExitSeat()
    {
        transform.SetParent(null);

        // Re-parent mesh back to player
        if (busDriver != null)
        {
            busDriver.SetParent(transform);
            busDriver.localPosition = Vector3.zero;
            busDriver.localRotation = Quaternion.identity;
        }

        // Restore original LOCAL camera position
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraLocalPos;
            Debug.Log($"Camera restored to local position: {cameraTransform.localPosition}");
        }

        // RE-ENABLE CameraShake when exiting
        if (cameraShake != null)
        {
            cameraShake.enabled = true;
        }

        if (exitPoint != null)
            transform.position = exitPoint.position;

        playerMovement.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = true;

        if (bus != null)
            bus.playerDriving = false;

        isSeated = false;
        Debug.Log("Exited seat");

        // Play idle animation
        SetAnimation(false, false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DriverSeat"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DriverSeat"))
        {
            playerInRange = false;
        }
    }

    // Helper function to set animations
    void SetAnimation(bool walking, bool sitting)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isSitting", sitting);
    }
}

