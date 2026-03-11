using UnityEngine;

public class GhostPassenger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    [Header("Dialogue")]
    public DialogueData passengerDialogue;

    [Header("UI")]
    public GameObject interactPrompt; // Assign your "F to interact with passenger" UI here

    private Transform player;
    private bool playerInRange = false;

    void Start()
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

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
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

        bool canShowPrompt =
            playerInRange &&
            DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive();

        interactPrompt.SetActive(canShowPrompt);
    }

    private void HandleInteraction()
    {
        if (!playerInRange)
            return;

        if (DialogueManager.Instance == null)
            return;

        if (DialogueManager.Instance.IsDialogueActive())
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            DialogueManager.Instance.StartDialogue(passengerDialogue);

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }
}