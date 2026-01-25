using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnaptoSeat : MonoBehaviour
{
    public Transform driverSeat;
    public Transform exitPoint;

    private PlayerMovement playerMovement;
    private bool playerInRange;
    private bool isSeated;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isSeated && playerInRange)
                SnapPlayerToSeat();
            else if (isSeated)
                ExitSeat();
        }
    }

    void SnapPlayerToSeat()
    {
        if (driverSeat == null)
        {
            GameObject seatObj = GameObject.FindGameObjectWithTag("DriverSeat");
            if (seatObj != null)
                driverSeat = seatObj.transform;
        }

        if (driverSeat == null) return;

        playerMovement.enabled = false;

        // Snap to seat
        transform.position = driverSeat.position;
        transform.rotation = driverSeat.rotation;
        transform.SetParent(driverSeat);

        // Rigidbody fix (if needed)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        isSeated = true;
        Debug.Log("Entered seat");
    }

    void ExitSeat()
    {
        transform.SetParent(null);

        if (exitPoint != null)
            transform.position = exitPoint.position;

        playerMovement.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        isSeated = false;
        Debug.Log("Exited seat");
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
}

