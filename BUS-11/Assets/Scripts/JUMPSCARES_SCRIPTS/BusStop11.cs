using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusStop11 : MonoBehaviour
{
    [Header("Light Flicker")]
    public Light[] lightsToFlicker; // Assign bus interior lights, headlights, etc.
    private float[] originalIntensities;
    public float flickerDuration = 1f; // How long lights flicker
    public float flickerSpeed = 0.5f; // How fast they flicker (lower = faster)

    [Header("Audio")]
    public AudioClip lightFlickerClip;
    public AudioClip whisperClip;
    public AudioSource lightAudioSource;

    private bool hasTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
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

            AudioManager.Instance.RegisterSFXSource(lightAudioSource);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FlickerLights(float duration)
    {
        if (lightsToFlicker == null || lightsToFlicker.Length == 0) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Complete blackout
            foreach (Light light in lightsToFlicker)
            {
                if (light != null)
                {
                    light.intensity = 0f;
                }
            }

            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));

            // Lights on (flickering intensity)
            for (int i = 0; i < lightsToFlicker.Length; i++)
            {
                if (lightsToFlicker[i] != null)
                {
                    lightsToFlicker[i].intensity = originalIntensities[i] * Random.Range(0.5f, 1f);
                }
            }
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

            elapsed += Random.Range(0.15f, 0.7f);
        }

        // Restore
        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
            {
                lightsToFlicker[i].intensity = originalIntensities[i];
            }
        }
    }

    void PlayLightFlicker()
    {
        if (lightAudioSource != null && lightFlickerClip != null)
        {
            lightAudioSource.volume = 1f;

            lightAudioSource.PlayOneShot(lightFlickerClip);
        }
    }

    void PlayWhisper()
    {
        if (lightAudioSource != null && whisperClip != null)
        {
            lightAudioSource.volume = 1f;

            lightAudioSource.PlayOneShot(whisperClip);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if bus passed this trigger
        if (other.CompareTag("Bus") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(FlickerLights(3f));

            PlayLightFlicker();
            PlayWhisper();
        }
    }
}
