using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDisableZone : MonoBehaviour
{
    [Header("Objects to Destroy")]
    public List<GameObject> objectsToDestroy; // objects to remove

    [Header("Objects to Enable")]
    public List<GameObject> objectsToEnable; // scene objects, initially inactive

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bus"))
        {
            Debug.Log("ENTERED ZONE");
            BusButton.doorDisabled = true;

            // Destroy assigned objects
            foreach (GameObject obj in objectsToDestroy)
            {
                if (obj != null)
                    Destroy(obj);
            }

            // Enable scene objects
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                    obj.SetActive(true);
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