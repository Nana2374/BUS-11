using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusBoundaryTrigger : MonoBehaviour
{
    public GameObject busBoundary;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the trigger: " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Object is NOT the player.");
            return;
        }

        Debug.Log("Player entered loading bay trigger!");

        if (busBoundary != null)
        {
            busBoundary.SetActive(true);
            Debug.Log("Bus boundary activated successfully!");
        }
        else
        {
            Debug.LogError("BusBoundary is NOT assigned in the inspector!");
        }
    }
}