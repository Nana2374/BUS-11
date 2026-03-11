using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;

    [Header("Settings")]
    public bool playOnce = true;
    public float triggerDelay = 0f;

    private bool hasPlayed = false;
    private bool playerInside = false;
    private float timer = 0f;

    private void Update()
    {
        if (!playerInside) return;

        if (playOnce && hasPlayed) return;

        timer += Time.deltaTime;

        if (timer >= triggerDelay)
        {
            if (!DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.StartDialogue(dialogue);
                hasPlayed = true;
                playerInside = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playOnce && hasPlayed) return;

        playerInside = true;
        timer = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        timer = 0f;
    }
}
