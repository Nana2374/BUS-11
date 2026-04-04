using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostItNoteInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    [Header("UI")]
    public GameObject postItUI;

    [Header("Optional")]
    public GameObject interactPrompt;

    private Transform player;
    private bool playerInRange = false;
    private bool isOpen = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }

        if (postItUI != null)
            postItUI.SetActive(false);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
            return;

        CheckPlayerDistance();
        UpdateInteractPrompt();
        HandleInteraction();
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionDistance;
    }

    private void UpdateInteractPrompt()
    {
        if (interactPrompt == null)
            return;

        // Show prompt only when close enough and note is not open
        interactPrompt.SetActive(playerInRange && !isOpen);
    }

    private void HandleInteraction()
    {
        // Press F  toggle open/close ONLY if player is in range
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (isOpen)
                CloseNote();
            else
                OpenNote();
        }

        // Allow ESC to close anytime when open
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseNote();
        }
    }

    public void Interact()
    {
        // Optional: still works if another interaction system calls this
        if (!playerInRange && !isOpen)
            return;

        if (isOpen)
            CloseNote();
        else
            OpenNote();
    }

    private void OpenNote()
    {
        if (postItUI != null)
            postItUI.SetActive(true);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseNote()
    {
        if (postItUI != null)
            postItUI.SetActive(false);

        isOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show prompt again if player is still nearby
        if (interactPrompt != null && playerInRange)
            interactPrompt.SetActive(true);
    }

    public void ShowPrompt()
    {
        if (interactPrompt != null && !isOpen && playerInRange)
            interactPrompt.SetActive(true);
    }

    public void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }
}
