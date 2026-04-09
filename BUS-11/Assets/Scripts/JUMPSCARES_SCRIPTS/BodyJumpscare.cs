using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyJumpscare : MonoBehaviour
{
    [Header("Jumpscare Visual")]
    public GameObject jumpscareImage;   // Your full-screen PNG

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpscareSound;

    [Header("Timing")]
    public float displayTime = 0.7f;

    public void PlayJumpscare()
    {
        StartCoroutine(ShowJumpscare());
    }

    IEnumerator ShowJumpscare()
    {
        // Show image
        if (jumpscareImage != null)
            jumpscareImage.SetActive(true);

        // Play sound
        if (audioSource != null && jumpscareSound != null)
            audioSource.PlayOneShot(jumpscareSound);

        yield return new WaitForSeconds(displayTime);

        // Hide image
        if (jumpscareImage != null)
            jumpscareImage.SetActive(false);
    }
}
