using UnityEngine;

public class GhostPassenger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    [Header("Dialogue")]
    public DialogueData passengerDialogue;

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

        if (playerInRange &&
            Input.GetKeyDown(KeyCode.F) &&
            !DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.StartDialogue(passengerDialogue);
        }
    }
}