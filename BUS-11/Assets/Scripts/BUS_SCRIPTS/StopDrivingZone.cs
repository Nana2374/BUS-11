using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopDrivingZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BusController bus = other.GetComponent<BusController>();

        if (bus != null)
        {
            bus.StopDriving();
        }
    }
}
