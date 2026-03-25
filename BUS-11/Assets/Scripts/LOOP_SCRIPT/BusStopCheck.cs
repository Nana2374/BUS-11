using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusStopCheck : MonoBehaviour
{
    public PassengerController passenger;
    public Transform resetPoint;

    private void OnTriggerEnter(Collider other)
    {
        BusResetController bus = other.GetComponentInParent<BusResetController>();
        if (bus == null) return;

        if (!passenger.HasBeenPickedUp)
        {
            Debug.Log("Passenger missed → RESET");
            StartCoroutine(bus.ResetBusRoutine(resetPoint));
        }
        else
        {
            Debug.Log("Passenger picked → continue");
        }
    }
}
