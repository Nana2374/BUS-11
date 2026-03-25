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
            monologueText.text = monologueData.nodes[i].dialogueLine;
            yield return new WaitForSeconds(lineDuration);
        }

        monologueText.text = "";
        monologuePanel.SetActive(false);
        monologueCoroutine = null;
    }
}

