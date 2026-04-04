using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostItNoteInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    [Header("UI")]
    public GameObject postItUI;


    [Header("Other UI To Hide")]
    public GameObject driverUI;

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
    }

    private void Update()
    {
        if (player == null)
            return;

        CheckPlayerDistance();

        if (isOpen && (Input.GetMouseButtonDown(0)))
        {
            CloseNote();
        }
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionDistance;
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

        // Hide driver UI
        if (driverUI != null)
            driverUI.SetActive(false);


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

        isOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}