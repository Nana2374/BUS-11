using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDisableZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bus"))
        {
            Debug.Log("ENTERED ZONE");

            BusButton.doorDisabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bus"))
        {
            Debug.Log("EXITED ZONE");
            BusButton.doorDisabled = false;
        }
    }
}
