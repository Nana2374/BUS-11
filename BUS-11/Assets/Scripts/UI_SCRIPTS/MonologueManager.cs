using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MonologueManager : MonoBehaviour
{
    public static MonologueManager Instance;

    [Header("UI")]
    public GameObject monologuePanel;
    public TextMeshProUGUI monologueText;
    public TextMeshProUGUI speakerNameText;

    [Header("Settings")]
    public float lineDuration = 3f;

    private Coroutine monologueCoroutine;

    void Awake()
    {
        Instance = this;

        if (monologuePanel != null)
            monologuePanel.SetActive(false);
    }

    public void PlayMonologue(DialogueData monologueData)
    {
        if (monologueData == null || monologueData.nodes == null || monologueData.nodes.Count == 0)
            return;

        if (monologueCoroutine != null)
            StopCoroutine(monologueCoroutine);

        monologueCoroutine = StartCoroutine(PlayMonologueRoutine(monologueData));
    }

    IEnumerator PlayMonologueRoutine(DialogueData monologueData)
    {
        monologuePanel.SetActive(true);

        for (int i = 0; i < monologueData.nodes.Count; i++)
        {
            DialogueNode node = monologueData.nodes[i];

            // SET SPEAKER NAME
            switch (node.speaker)
            {
                case SpeakerType.Driver:
                    speakerNameText.text = "DRIVER";
                    break;

                case SpeakerType.Ghost:
                    speakerNameText.text = "GHOST";
                    break;

                case SpeakerType.Unknown:
                    speakerNameText.text = "???";
                    break;

                case SpeakerType.SchoolGirl:
                    speakerNameText.text = "SCHOOL GIRL";
                    break;

                case SpeakerType.SchoolBoyA:
                    speakerNameText.text = "SCHOOL BOY A";
                    break;

                case SpeakerType.SchoolBoyB:
                    speakerNameText.text = "SCHOOL BOY B";
                    break;

                case SpeakerType.SchoolBoyC:
                    speakerNameText.text = "SCHOOL BOY C";
                    break;

                case SpeakerType.SchoolBoyAandC:
                    speakerNameText.text = "SCHOOL BOY A & C";
                    break;

                case SpeakerType.Nurse:
                    speakerNameText.text = "NURSE";
                    break;

                case SpeakerType.OfficeWorker:
                    speakerNameText.text = "OFFICE WORKER";
                    break;

                case SpeakerType.ElderlyMan:
                    speakerNameText.text = "ELDERLY MAN";
                    break;

                case SpeakerType.ElderlyWoman:
                    speakerNameText.text = "ELDERLY WOMAN";
                    break;

                case SpeakerType.Everyone:
                    speakerNameText.text = "ALL";
                    break;

            }

            monologueText.text = node.dialogueLine;

            yield return new WaitForSeconds(lineDuration);
        }

        monologueText.text = "";
        speakerNameText.text = ""; // CLEAR AFTER
        monologuePanel.SetActive(false);
        monologueCoroutine = null;
    }
}

