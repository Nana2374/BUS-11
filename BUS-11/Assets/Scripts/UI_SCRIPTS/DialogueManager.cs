using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public static DialogueManager Instance;

    [Header("Seat Lock Reference")]
    public SnaptoSeat snapToSeat;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;

    [Header("Choice UI")]
    public GameObject choicesPanel;
    public Button[] choiceButtons;              // Assign your choice buttons here
    public TextMeshProUGUI[] choiceTexts;      // Assign TMP text inside each button

    public MouseLook mouseLook;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;

    private DialogueData currentDialogue;
    private int currentNodeIndex;
    private bool isTyping;
    public bool dialogueActive;
    private bool isChoosing;

    private Coroutine typingCoroutine;

    void Awake()
    {
        Instance = this;

        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (isChoosing)
        {
            // No more W/S input
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];

            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                if (currentNode.choices != null && currentNode.choices.Count > 0)
                {
                    ShowChoices(currentNode.choices);
                }
                else
                {
                    GoToNextLinearNode();
                }
            }
        }
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.nodes == null || dialogue.nodes.Count == 0)
        {
            Debug.LogWarning("Dialogue is empty or null.");
            return;
        }

        currentDialogue = dialogue;
        currentNodeIndex = 0;
        dialogueActive = true;

        if (snapToSeat != null && snapToSeat.isSeated)
        {
            snapToSeat.LockSeat();
        }

        dialoguePanel.SetActive(true);
        choicesPanel.SetActive(false);

        ShowCurrentNode();
    }

    void ShowCurrentNode()
    {
        if (currentDialogue == null || currentNodeIndex < 0 || currentNodeIndex >= currentDialogue.nodes.Count)
        {
            EndDialogue();
            return;
        }

        DialogueNode node = currentDialogue.nodes[currentNodeIndex];

        switch (node.speaker)
        {
            case SpeakerType.Driver:
                speakerNameText.text = "DRIVER";
                speakerNameText.color = Color.yellow;
                break;

            case SpeakerType.Ghost:
                speakerNameText.text = "GHOST";
                speakerNameText.color = Color.yellow;
                break;

            case SpeakerType.Unknown:
                speakerNameText.text = "???";
                speakerNameText.color = Color.yellow;
                break;

            case SpeakerType.SchoolGirl:
                speakerNameText.text = "SCHOOL GIRL";
                speakerNameText.color = Color.yellow;
                break;

            case SpeakerType.SchoolBoyA:
                speakerNameText.text = "SCHOOL BOY A";
                speakerNameText.color = Color.yellow;
                break;

            case SpeakerType.SchoolBoyB:
                speakerNameText.text = "SCHOOL BOY B";
                speakerNameText.color = Color.yellow;
                break;

            case SpeakerType.SchoolBoyC:
                speakerNameText.text = "SCHOOL BOY C";
                speakerNameText.color = Color.yellow;
                break;

            case SpeakerType.Nurse:
                speakerNameText.text = "NURSE";
                speakerNameText.color = Color.yellow;
                break;


            case SpeakerType.OfficeWorker:
                speakerNameText.text = "OFFICE WORKER";
                speakerNameText.color = Color.yellow;
                break;


            case SpeakerType.ElderlyMan:
                speakerNameText.text = "ELDERLY MAN";
                speakerNameText.color = Color.yellow;
                break;


            case SpeakerType.ElderlyWoman:
                speakerNameText.text = "ELDERLY WOMAN";
                speakerNameText.color = Color.yellow;
                break;

        }


        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(node.dialogueLine));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        isChoosing = false;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentDialogue.nodes[currentNodeIndex].dialogueLine;
        isTyping = false;
    }

    void GoToNextLinearNode()
    {
        if (currentDialogue == null || currentNodeIndex < 0 || currentNodeIndex >= currentDialogue.nodes.Count)
        {
            EndDialogue();
            return;
        }

        DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];

        // First priority: use explicitly assigned next node
        if (currentNode.nextNodeIndex >= 0 && currentNode.nextNodeIndex < currentDialogue.nodes.Count)
        {
            currentNodeIndex = currentNode.nextNodeIndex;
            ShowCurrentNode();
            return;
        }

        // If no next node is assigned, end dialogue
        EndDialogue();
    }

    void ShowChoices(List<DialogueChoice> choices)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mouseLook != null)
            mouseLook.canLook = false;

        if (choices == null || choices.Count == 0)
            return;

        isChoosing = true;
        choicesPanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = choices[i].choiceText;

                int choiceIndex = i; // Important: local copy for button click
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectChoice(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void SelectChoice(int choiceIndex)
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        List<DialogueChoice> choices = node.choices;

        if (choiceIndex < 0 || choiceIndex >= choices.Count)
            return;

        int nextNode = choices[choiceIndex].nextNodeIndex;

        choicesPanel.SetActive(false);
        isChoosing = false;

        if (nextNode < 0 || nextNode >= currentDialogue.nodes.Count)
        {
            EndDialogue();
            return;
        }

        currentNodeIndex = nextNode;
        ShowCurrentNode();
    }

    void EndDialogue()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mouseLook != null)
            mouseLook.canLook = true;

        if (snapToSeat != null)
            snapToSeat.UnlockSeat();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueActive = false;
        isTyping = false;
        isChoosing = false;

        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);
        dialogueText.text = "";
        speakerNameText.text = "";

        currentDialogue = null;
        currentNodeIndex = 0;
    }
}