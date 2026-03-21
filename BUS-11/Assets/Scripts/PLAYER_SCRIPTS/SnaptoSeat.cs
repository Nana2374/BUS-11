using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnaptoSeat : MonoBehaviour
{
    public Transform driverSeat;
    public Transform exitPoint;
    public BusController bus;
    public Transform busDriverP;
    public Transform busDriverMesh; // Actual mesh (BusDriver_Idle)

    [Header("Animation")]
    public Animator animator;

    private PlayerMovement playerMovement;
    private bool playerInRange = false;
    public bool isSeated = false;

    [Header("Camera Setup")]
    public Camera mainCamera; // Use Camera component instead of Transform
    public CameraShake cameraShake; // Add reference to camera shake script

    [Header("UI Elements")]
    public GameObject drivingUI; // Assign your UI panel/images here

    // Store original positions
    private Vector3 originalParentLocalPos;
    private Quaternion originalParentLocalRot;
    private Vector3 originalMeshLocalPos;
    private Quaternion originalMeshLocalRot;
    private Transform cameraTransform;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        // Hide driving UI at start
        if (drivingUI != null)
        {
            drivingUI.SetActive(false);
        }

        // Store original Bus_Driver_P position
        if (busDriverP != null)
        {
            originalParentLocalPos = busDriverP.localPosition;
            originalParentLocalRot = busDriverP.localRotation;
            Debug.Log($"Original Bus_Driver_P position: {originalParentLocalPos}");

            // Auto-find the mesh child if not assigned
            if (busDriverMesh == null)
            {
                busDriverMesh = busDriverP.Find("BusDriver_Idle");
            }
        }

        // Store original BusDriver_Idle position (relative to Bus_Driver_P)
        if (busDriverMesh != null)
        {
            originalMeshLocalPos = busDriverMesh.localPosition;
            originalMeshLocalRot = busDriverMesh.localRotation;
            Debug.Log($"Original BusDriver_Idle position: {originalMeshLocalPos}");
        }

        if (driverSeat == null)
        {
            GameObject seatObj = GameObject.FindGameObjectWithTag("DriverSeat");
            if (seatObj != null)
                driverSeat = seatObj.transform;
        }

        // Store original camera position relative to Player
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Get camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
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
    }

    void SnapPlayerToSeat()
    {
        // ADD THIS CHECK AT THE START
        if (!playerInRange)
        {
            Debug.Log("Cannot enter seat - not in range!");
            return;
        }

        if (driverSeat == null) return;

        playerMovement.enabled = false;

        // Snap to seat
        transform.position = driverSeat.position + new Vector3(0f, 0.08f, 0.08f); //Snap Player to seat 
        transform.rotation = driverSeat.rotation;
        transform.SetParent(driverSeat);

        Debug.Log($"Snapped to seat at position: {transform.localPosition}, rotation: {transform.rotation.eulerAngles}");

        // Unparent Bus_Driver_P so it doesn't rotate
        if (busDriverP != null)
        {
            busDriverP.SetParent(driverSeat); // Parent to seat
            busDriverP.localPosition = new Vector3(0f, -0.35f, 0.07f); // Position relative to seat
            busDriverP.localRotation = Quaternion.identity;
            Debug.Log("Bus_Driver_P positioned at seat");
        }

        // Position the actual mesh (BusDriver_Idle)
        if (busDriverMesh != null)
        {
            busDriverMesh.localPosition = new Vector3(0f, 0f, 0f); // Position relative to Bus_Driver_P
            busDriverMesh.localRotation = Quaternion.identity;
            Debug.Log("BusDriver_Idle positioned");

            // Change camera to seated position (LOCAL position - relative to Player)
            /*if (cameraTransform != null)
            {
                cameraTransform.localPosition = seatedCameraLocalPos;
                Debug.Log($"Camera seated at local position: {cameraTransform.localPosition}");
            }*/

            // DISABLE CameraShake when seated
            if (cameraShake != null)
            {
                cameraShake.enabled = false;
            }

            // SHOW DRIVING UI
            if (drivingUI != null)
            {
                drivingUI.SetActive(true);
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
    }

        void ExitSeat()
        {
            transform.SetParent(null);

            // Move to exit point FIRST
            if (exitPoint != null)
            {
                transform.position = exitPoint.position;
                transform.rotation = Quaternion.identity; // Reset rotation
            }

            // Restore original LOCAL camera position
            /*if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalCameraLocalPos;
                Debug.Log($"Camera restored to local position: {cameraTransform.localPosition}");
            }*/

            // Re-parent Bus_Driver_P and restore ORIGINAL position
            if (busDriverP != null)
            {
                busDriverP.SetParent(transform); // Re-parent to Player
                busDriverP.localPosition = originalParentLocalPos + new Vector3(0f, 0f, -0.3f); // Restore exact original
                busDriverP.localRotation = originalParentLocalRot;
                Debug.Log($"Bus_Driver_P restored to: {busDriverP.localPosition}");
            }

            // Restore BusDriver_Idle mesh to ORIGINAL position (relative to Bus_Driver_P)
            if (busDriverMesh != null)
            {
                busDriverMesh.localPosition = originalMeshLocalPos; // Restore exact original
                busDriverMesh.localRotation = originalMeshLocalRot;
                Debug.Log($"BusDriver_Idle restored to: {busDriverMesh.localPosition}");
            }

            // RE-ENABLE CameraShake when exiting
            if (cameraShake != null)
            {
                cameraShake.enabled = true;
            }

        // HIDE DRIVING UI
        if (drivingUI != null)
        {
            drivingUI.SetActive(false);
        }

        playerMovement.enabled = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;

            Collider col = GetComponent<Collider>();
            if (col) col.enabled = true;

            if (bus != null)
                bus.playerDriving = false;

            playerInRange = false;

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

    void OnGUI()
    {
        // Show prompt when in range and not seated
        if (playerInRange && !isSeated)
        {
            // Create a style for the text
            GUIStyle style = new GUIStyle();
            style.fontSize = 15;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            string message = "[Left Shift] to enter Driver's Seat";

            // Calculate position (center bottom of screen)
            float xPos = Screen.width / 2 - 150;
            float yPos = Screen.height - 150;

            // Draw main text
            GUI.Label(new Rect(xPos, yPos, 300, 40), message, style);
        }
    }
}

