using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    private void Awake()
    {
        Instance = this;

        if (fadeGroup != null)
            fadeGroup.alpha = 0f;
    }

    public IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 1f;
    }

    public IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 0f;
    }

    public IEnumerator FadeOutIn(float holdTime = 0.2f)
    {
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(holdTime);
        yield return StartCoroutine(FadeIn());
    }
}
