using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusResetController : MonoBehaviour
{

    public Rigidbody rb;
    public BusController busController;

    public void ResetBus(Transform resetPoint)
    {
        if (resetPoint == null || rb == null) return;

        // Put bus in park
        if (busController != null)
            busController.currentGear = 0;

        // Stop wheel movement
        if (busController != null)
        {
            busController.frontLeft.motorTorque = 0f;
            busController.frontRight.motorTorque = 0f;
            busController.rearLeft.motorTorque = 0f;
            busController.rearRight.motorTorque = 0f;

            busController.frontLeft.brakeTorque = 0f;
            busController.frontRight.brakeTorque = 0f;
            busController.rearLeft.brakeTorque = 0f;
            busController.rearRight.brakeTorque = 0f;
        }

        // Clear physics first
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();

        // Teleport the rigidbody itself
        rb.position = resetPoint.position;
        rb.rotation = resetPoint.rotation;

        // Also sync transform just in case
        transform.position = resetPoint.position;
        transform.rotation = resetPoint.rotation;

        Physics.SyncTransforms();

        // Freeze again for park
        rb.constraints = RigidbodyConstraints.FreezeAll;

        Debug.Log("Bus reset to stop start.");
    }
}

