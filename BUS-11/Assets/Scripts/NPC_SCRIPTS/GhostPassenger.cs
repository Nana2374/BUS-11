using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPassenger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.F;

    [Header("Dialogue")]
    [TextArea]
    public string[] dialogueLines;

    private Transform player;
    private bool playerInRange = false;
    private bool dialogueOpen = false;

    void Start()
    {
        // Find player (make sure your player has tag "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure player has tag 'Player'");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionDistance;

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (!playerInRange) return;

        if (!dialogueOpen)
        {
            OpenDialogue();
        }
        else
        {
            CloseDialogue();
        }
    }

    void OpenDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogueLines);
    }

    void CloseDialogue()
    {
        dialogueOpen = false;
        Debug.Log("Dialogue Closed");
    }
}
