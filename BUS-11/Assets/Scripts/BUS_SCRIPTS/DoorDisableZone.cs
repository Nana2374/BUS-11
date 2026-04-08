using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDisableZone : MonoBehaviour
{

    public GhostPassenger ghostPassenger;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bus"))
        {
            Debug.Log("ENTERED ZONE");

            BusButton.doorDisabled = true;

            if (ghostPassenger != null)
            {
                ghostPassenger.SwitchDialogue();
            }
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
