using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearClickDrag : MonoBehaviour, IInteractable
{
    [Header("Gear Positions")]
    public Transform parkPosition;      // P
    public Transform reversePosition;   // R
    public Transform gear1Position;     // 1
    public Transform gear2Position;     // 2
    public Transform gear3Position;     // 3

    [Header("Settings")]
    public float snapDistance = 0.3f;   // How close to snap to gear position
    public BusController busController;
    public LayerMask gearPlaneLayer;    // Layer for invisible plane to drag on

    private bool isDragging = false;
    private Camera cam;
    private Quaternion initialRotation;
    private Plane dragPlane;
    private Vector3 offset;

    void Start()
    {
        cam = Camera.main;
        initialRotation = transform.localRotation;

        if (busController == null)
        {
            busController = FindObjectOfType<BusController>();
        }

        // Create a plane for dragging (at gear stick's Y position)
        dragPlane = new Plane(Vector3.up, transform.position);
    }

    void Update()
    {
        if (isDragging)
        {
            DragGearStick();
        }
    }

    public void Interact()
    {
        // Start dragging when clicked
        if (!isDragging)
        {
            StartDragging();
        }
    }

    void StartDragging()
    {
        isDragging = true;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            offset = transform.position - hitPoint;
        }

        Debug.Log("Started dragging gear stick");
    }

    void DragGearStick()
    {
        // Stop dragging when mouse released
        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
            return;
        }

        // Move gear stick with mouse
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            transform.position = hitPoint + offset;

            // Keep original rotation
            transform.localRotation = initialRotation;

            // Check if near any gear position
            CheckGearSnap();
        }
    }

    void StopDragging()
    {
        isDragging = false;

        // Snap to nearest gear position
        SnapToNearestGear();

        Debug.Log("Stopped dragging gear stick");
    }

    void CheckGearSnap()
    {
        // Visual feedback - could add highlighting here
    }

    void SnapToNearestGear()
    {
        float closestDistance = float.MaxValue;
        int closestGear = busController.currentGear;
        Vector3 closestPosition = transform.position;

        // Check distance to each gear position
        if (parkPosition != null)
        {
            float dist = Vector3.Distance(transform.position, parkPosition.position);
            if (dist < closestDistance && dist < snapDistance)
            {
                closestDistance = dist;
                closestGear = 0;
                closestPosition = parkPosition.position;
            }
        }

        if (reversePosition != null)
        {
            float dist = Vector3.Distance(transform.position, reversePosition.position);
            if (dist < closestDistance && dist < snapDistance)
            {
                closestDistance = dist;
                closestGear = -1;
                closestPosition = reversePosition.position;
            }
        }

        if (gear1Position != null)
        {
            float dist = Vector3.Distance(transform.position, gear1Position.position);
            if (dist < closestDistance && dist < snapDistance)
            {
                closestDistance = dist;
                closestGear = 1;
                closestPosition = gear1Position.position;
            }
        }

        if (gear2Position != null)
        {
            float dist = Vector3.Distance(transform.position, gear2Position.position);
            if (dist < closestDistance && dist < snapDistance)
            {
                closestDistance = dist;
                closestGear = 2;
                closestPosition = gear2Position.position;
            }
        }

        if (gear3Position != null)
        {
            float dist = Vector3.Distance(transform.position, gear3Position.position);
            if (dist < closestDistance && dist < snapDistance)
            {
                closestDistance = dist;
                closestGear = 3;
                closestPosition = gear3Position.position;
            }
        }

        // Snap to position
        transform.position = closestPosition;

        // Change gear in bus controller
        if (closestGear != busController.currentGear)
        {
            busController.currentGear = closestGear;
            Debug.Log("Shifted to gear: " + closestGear);
        }
    }
}
