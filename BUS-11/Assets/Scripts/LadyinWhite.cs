using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadyinWhite : MonoBehaviour
{
    [Header("References")]
    public PassengerController passengerController;
    public BusController busController;

    [Header("Light Flicker")]
    public Light[] lightsToFlicker; // Assign bus interior lights, headlights, etc.
    public float flickerDuration = 10f; // How long lights flicker
    public float flickerSpeed = 0.1f; // How fast they flicker (lower = faster)

    [Header("Red Light Effect")]
    public Color redColor = new Color(1f, 0f, 0f); // Pure red
    public bool enableRedLights = true;

    [Header("Red Light Sound")]
    public AudioSource audioSource;     // Drag your AudioSource here
    public AudioClip redLightSFX;       // Drag your sound clip here


    [Header("Destroy Settings")]
    public float destroyDelay = 1f; // Delay after sequence before destroying

    private bool hasTriggered = false;
    private float[] originalIntensities;
    private Color[] originalColors;

    // Start is called before the first frame update
    void Start()
    {
        // Store original light intensities
        if (lightsToFlicker != null && lightsToFlicker.Length > 0)
        {
            originalIntensities = new float[lightsToFlicker.Length];
            originalColors = new Color[lightsToFlicker.Length];
            for (int i = 0; i < lightsToFlicker.Length; i++)
            {
                if (lightsToFlicker[i] != null)
                {
                    originalIntensities[i] = lightsToFlicker[i].intensity;
                    originalColors[i] = lightsToFlicker[i].color;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasTriggered && passengerController.CurrentState == PassengerController.PassengerState.Seated && busController.rb.velocity.magnitude > 0.1f)
        {
            hasTriggered = true;

            StartCoroutine(GhostSequence());
        }
    }

    IEnumerator GhostSequence()
    {
        // Wait 2 seconds
        yield return new WaitForSeconds(2f);

        Debug.Log("Ghost is pulling the wheel!");

        if (enableRedLights)
        {
            SetAllLightColors(redColor);

            PlayRedLightSound();
        }

        Coroutine flickerCoroutine = StartCoroutine(FlickerLights(flickerDuration));

        // Duration, steer force, acceleration force
        busController.TriggerGhostEvent(10f, 10f, 10f);

        // Wait for flickering to finish
        yield return flickerCoroutine;

        Debug.Log("Ghost sequence complete!");

        RestoreOriginalColors();

        yield return new WaitForSeconds(destroyDelay);

        // Destroy this gameobject (the ghost)
        Destroy(gameObject);
    }

    IEnumerator FlickerLights(float duration)
    {
        if (lightsToFlicker == null || lightsToFlicker.Length == 0) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Complete blackout
            SetAllLights(0f);
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

    void SetAllLights(float intensity)
    {
        foreach (Light light in lightsToFlicker)
        {
            if (light != null)
            {
                light.intensity = intensity;
            }
        }
    }

    void SetAllLightColors(Color color)
    {
        foreach (Light light in lightsToFlicker)
        {
            if (light != null)
            {
                light.color = color;
            }
        }
        Debug.Log("Lights changed to red");
    }

    void RestoreOriginalColors()
    {
        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
            {
                lightsToFlicker[i].color = originalColors[i];
            }
        }
        Debug.Log("Lights restored to original colors");
    }
    void PlayRedLightSound()
    {
        if (audioSource != null && redLightSFX != null)
        {
            audioSource.clip = redLightSFX;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    void StopRedLightSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
