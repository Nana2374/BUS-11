using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDemo : MonoBehaviour
{
    [Header("Settings")]
    public bool triggerOnce = true; // Only trigger once

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if ((other.CompareTag("Player")) || (other.CompareTag("Bus")))
        {
            // Prevent multiple triggers
            if (triggerOnce && hasTriggered)
            {
                return;
            }

            hasTriggered = true;

            Debug.Log("Player reached end of demo zone!");

            // Call end of demo
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndofDemo();
            }
            else
            {
                Debug.LogError("GameManager not found!");
            }
        }
    }
}
