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

    [Header("Other UI To Hide")]
    public GameObject driverUI;
    public GameObject driverChoicesUI; // optional

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

        interactPrompt.SetActive(playerInRange && !isOpen);
    }

    private void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (isOpen)
                CloseNote();
            else
                OpenNote();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseNote();
        }
    }

    public void Interact()
    {
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

        // Hide driver UI
        if (driverUI != null)
            driverUI.SetActive(false);

        if (driverChoicesUI != null)
            driverChoicesUI.SetActive(false);

        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseNote()
    {
        if (postItUI != null)
            postItUI.SetActive(false);

        // Bring driver UI back
        if (driverUI != null)
            driverUI.SetActive(true);

        if (driverChoicesUI != null)
            driverChoicesUI.SetActive(true);

        isOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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