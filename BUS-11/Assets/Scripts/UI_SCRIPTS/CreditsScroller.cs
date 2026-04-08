using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    public RectTransform creditsPanel;  // The parent panel containing all text objects
    public float scrollSpeed = 50f;     // Pixels per second
    public float waitAfterEnd = 2f;     // Pause at the end

    private float startY;
    private float endY;

    void Start()
    {
        if (creditsPanel == null)
        {
            Debug.LogError("Credits panel not assigned!");
            return;
        }

        startY = -Screen.height; // start off-screen at bottom
        endY = Screen.height + creditsPanel.rect.height; // scroll past top

        creditsPanel.anchoredPosition = new Vector2(0, startY);

        StartCoroutine(ScrollCredits());
    }

    IEnumerator ScrollCredits()
    {
        while (creditsPanel.anchoredPosition.y < endY)
        {
            creditsPanel.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        // wait a bit at the end
        yield return new WaitForSeconds(waitAfterEnd);
    }
}
