using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GhostLadyJumpscare : MonoBehaviour
{
    [Header("References")]
    public PassengerController ghostLady; // Drag the ghost lady passenger here
    public GameObject jumpscareCanvas;   // The Canvas GameObject
    public Animator ghostAnimator;       // Animator on GhostFaceImage

    [Header("Light Flicker")]
    public Light[] lightsToFlicker; // Assign bus interior lights, headlights, etc.
    private float[] originalIntensities;

    [Header("Audio")]
    public AudioClip screamClip;
    public AudioSource screamAudioSource;

    [Header("Settings")]
    public float jumpscareDuration = 1f;  // How long it stays on screen

    private bool isPlaying = false;
    private bool hasTriggered = false;

    void Start()
    {
        jumpscareCanvas.SetActive(false); // Hidden at start

        // Store original light intensities
        if (lightsToFlicker != null && lightsToFlicker.Length > 0)
        {
            originalIntensities = new float[lightsToFlicker.Length];
            for (int i = 0; i < lightsToFlicker.Length; i++)
            {
                if (lightsToFlicker[i] != null)
                {
                    originalIntensities[i] = lightsToFlicker[i].intensity;
                }
            }
        }

        // Scream audio source (attach to ghost lady for positioning)
        if (screamAudioSource == null && ghostLady != null)
        {
            screamAudioSource = ghostLady.gameObject.AddComponent<AudioSource>();
        }

        AudioManager.Instance.RegisterSFXSource(screamAudioSource);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if bus passed this trigger
        if (other.CompareTag("Bus") && !hasTriggered)
        {
            hasTriggered = true;

            // Check if ghost lady is on the bus
            if (ghostLady != null && ghostLady.CurrentState != PassengerController.PassengerState.Seated)
            {
                Debug.Log("Ghost lady not picked up! Starting haunting...");
                StartCoroutine(OffLights());
                TriggerJumpscare();
            }
            else
            {
                Debug.Log("Ghost lady is safe on the bus.");
                Destroy(gameObject);
            }
        }
    }

    public void TriggerJumpscare()
    {
        if (!isPlaying)
            StartCoroutine(PlayJumpscare());
    }

    void PlayScream()
    {
        if (screamAudioSource != null && screamClip != null)
        {
            screamAudioSource.pitch = Random.Range(0.9f, 1.1f);
            screamAudioSource.volume = 1f;

            screamAudioSource.PlayOneShot(screamClip);
        }
    }

    IEnumerator PlayJumpscare()
    {
        isPlaying = true;

        yield return new WaitForSeconds(0.5f);

        // 1. Show the canvas
        jumpscareCanvas.SetActive(true);

        // 3. Play animation + sound
        ghostAnimator.Play("GhostLadyJumpscare", 0, 0f); // Force restart
        PlayScream();

        // 4. Wait for jumpscare duration
        yield return new WaitForSeconds(jumpscareDuration);

        // 6. Hide canvas
        jumpscareCanvas.SetActive(false);
        isPlaying = false;
    }

    IEnumerator OffLights()
    {
        if (lightsToFlicker == null || lightsToFlicker.Length == 0) yield break;

        // Turn OFF all lights
        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
                lightsToFlicker[i].intensity = 0f;
        }

        // Wait 1 second
        yield return new WaitForSeconds(2f);

        // Restore all lights
        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
                lightsToFlicker[i].intensity = originalIntensities[i];
        }
    }
}
