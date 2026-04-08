using UnityEngine;

public class GhostPassenger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    [Header("Dialogue")]
    public DialogueData passengerDialogue;



    [Header("UI")]
    public GameObject interactPrompt;

    private Transform player;
    private bool playerInRange = false;
    private bool hasInteracted = false; // NEW

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
            !hasInteracted && // NEW
            DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive();

        interactPrompt.SetActive(canShowPrompt);
    }

    private void HandleInteraction()
    {
        if (!playerInRange)
            return;

        if (hasInteracted) // NEW
            return;

        if (DialogueManager.Instance == null)
            return;

        if (DialogueManager.Instance.IsDialogueActive())
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            hasInteracted = true; // NEW
            DialogueManager.Instance.StartDialogue(passengerDialogue);

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

}