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

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;

    private string[] currentLines;
    private int currentIndex;
    private bool isTyping;
    private bool dialogueActive;

    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLines[currentIndex];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    public void StartDialogue(string[] lines)
    {
        dialogueActive = true;
        dialoguePanel.SetActive(true);

        currentLines = lines;
        currentIndex = 0;

        ShowLine();
    }

    void ShowLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine(currentLines[currentIndex]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // If this is the FINAL line
        if (currentIndex == currentLines.Length - 1)
        {
            yield return new WaitForSeconds(2f); // Let it sit...
            EndDialogue();
        }
    }

    void NextLine()
    {
        currentIndex++;

        if (currentIndex >= currentLines.Length)
            return; // DO NOT close here anymore

        ShowLine();
    }

    void EndDialogue()
    {
        StopAllCoroutines();

        dialogueActive = false;
        isTyping = false;

        dialoguePanel.SetActive(false);
        dialogueText.text = "";

        currentLines = null;
        currentIndex = 0;
    }
}
