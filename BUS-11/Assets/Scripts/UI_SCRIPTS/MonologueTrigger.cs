using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonologueTrigger : MonoBehaviour
{
    public DialogueData monologueData;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Bus"))
        {
            triggered = true;

            if (MonologueManager.Instance != null)
            {
                MonologueManager.Instance.PlayMonologue(monologueData);
            }
        }
    }
}
