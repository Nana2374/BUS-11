using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Choice UI")]
    public GameObject choicesPanel;
    public TextMeshProUGUI[] choiceTexts; // Assign 2, 3, or 4 choice text objects in Inspector

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;

    private DialogueData currentDialogue;
    private int currentNodeIndex;
    private bool isTyping;
    private bool dialogueActive;
    private bool isChoosing;
    private int selectedChoiceIndex;

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
            HandleChoiceInput();
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
                // If node has choices, show them
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
        int nextIndex = currentNodeIndex + 1;

        if (nextIndex >= currentDialogue.nodes.Count)
        {
            EndDialogue();
            return;
        }

        currentNodeIndex = nextIndex;
        ShowCurrentNode();
    }

    void ShowChoices(List<DialogueChoice> choices)
    {
        if (choices == null || choices.Count == 0)
            return;

        isChoosing = true;
        selectedChoiceIndex = 0;
        choicesPanel.SetActive(true);

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (i < choices.Count)
            {
                choiceTexts[i].gameObject.SetActive(true);
                choiceTexts[i].text = choices[i].choiceText;
            }
            else
            {
                choiceTexts[i].gameObject.SetActive(false);
            }
        }

        UpdateChoiceHighlight();
    }

    void HandleChoiceInput()
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        List<DialogueChoice> choices = node.choices;

        if (Input.GetKeyDown(KeyCode.W))
        {
            selectedChoiceIndex--;
            if (selectedChoiceIndex < 0)
                selectedChoiceIndex = choices.Count - 1;

            UpdateChoiceHighlight();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            selectedChoiceIndex++;
            if (selectedChoiceIndex >= choices.Count)
                selectedChoiceIndex = 0;

            UpdateChoiceHighlight();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SelectChoice();
        }
    }

    void UpdateChoiceHighlight()
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        List<DialogueChoice> choices = node.choices;

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (i < choices.Count)
            {
                if (i == selectedChoiceIndex)
                    choiceTexts[i].text = "> " + choices[i].choiceText;
                else
                    choiceTexts[i].text = choices[i].choiceText;
            }
        }
    }

    void SelectChoice()
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        List<DialogueChoice> choices = node.choices;

        int nextNode = choices[selectedChoiceIndex].nextNodeIndex;

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
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueActive = false;
        isTyping = false;
        isChoosing = false;

        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);
        dialogueText.text = "";

        currentDialogue = null;
        currentNodeIndex = 0;
    }
}