using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayHover()
    {
        if (hoverClip != null)
            uiAudioSource.PlayOneShot(hoverClip);
    }

    public void PlayClick()
    {
        if (clickClip != null)
            uiAudioSource.PlayOneShot(clickClip);
    }
}
