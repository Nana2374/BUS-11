using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryWarningTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData warningDialogue;

    [Header("Settings")]
    public bool playOnce = false;
    public float cooldown = 2f;

    private bool hasPlayed = false;
    private float lastTriggerTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playOnce && hasPlayed)
            return;

        if (Time.time < lastTriggerTime + cooldown)
            return;

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager instance not found.");
            return;
        }

        if (DialogueManager.Instance.IsDialogueActive())
            return;

        DialogueManager.Instance.StartDialogue(warningDialogue);

        hasPlayed = true;
        lastTriggerTime = Time.time;

        Debug.Log("Boundary warning dialogue played.");
    }
}