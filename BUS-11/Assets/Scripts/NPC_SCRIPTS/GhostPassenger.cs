using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPassenger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    [Header("Dialogue")]
    [TextArea]
    public string[] dialogueLines;

    private Transform player;
    private bool playerInRange = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Player not found! Make sure player has tag 'Player'");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionDistance;

        // Only allow starting dialogue if:
        // 1. Player is in range
        // 2. Dialogue is NOT already active
        if (playerInRange &&
            Input.GetKeyDown(KeyCode.F) &&
            !DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.StartDialogue(dialogueLines);
        }
    }
}
