using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadyinWhite : MonoBehaviour
{
    [Header("References")]
    public PassengerController passengerController;
    public BusController busController;
    public GameObject[] handprintPlanes; // Drag all 20 handprint planes here

    [Header("Light Flicker")]
    public Light[] lightsToFlicker; // Assign bus interior lights, headlights, etc.
    public float flickerDuration = 7f; // How long lights flicker
    public float flickerSpeed = 0.1f; // How fast they flicker (lower = faster)

    [Header("Activation Settings")]
    public float activationInterval = 0.5f; // Time between each handprint appearing
    public float delayBeforeStart = 2f; // Delay before first handprint
    public float flashDuration = 0.2f;  // How long each pre-flash plane is visible
    public float flashInterval = 1f;

    [Header("Timing Curve")]
    public float startInterval = 0.5f;   // slow at start
    public float endInterval = 0.05f;     // very fast near end

    [Header("Red Light Effect")]
    public Color redColor = new Color(1f, 0f, 0f); // Pure red
    public bool enableRedLights = true;

    [Header("Red Light Sound")]
    public AudioSource audioSource;     // Drag your AudioSource here
    public AudioClip redLightSFX;       // Drag your sound clip here
    public AudioClip[] handSlapSounds;
    public AudioSource handprintAudioSource;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    public float volume = 1f;


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
        // Handprint audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (handprintAudioSource == null)
        {
            handprintAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Register both
        AudioManager.Instance.RegisterSFXSource(handprintAudioSource);
        AudioManager.Instance.RegisterSFXSource(audioSource);
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
        StartCoroutine(ActivateHandprints()); 


        // Duration, steer force, acceleration force
        busController.TriggerGhostEvent(6f, 10f, 10f);

        // Wait for flickering to finish
        yield return flickerCoroutine;

        Debug.Log("Ghost sequence complete!");

        RestoreOriginalColors();

        yield return new WaitForSeconds(destroyDelay);

        // Destroy this gameobject (the ghost)
        Destroy(gameObject);
    }

    IEnumerator ActivateHandprints()
    {
        int total = handprintPlanes.Length;

        for (int i = 0; i < total; i++)
        {
            GameObject handprint = handprintPlanes[i];
            if (handprint == null) continue;

            bool isLast = (i == total - 1);

            handprint.SetActive(true);

            // Normal slap
            handprintAudioSource.transform.position = handprint.transform.position;
            PlayHandSlapSound();

            // Calculate speed ramp (0 → 1 across sequence)
            float t = (float)i / (total - 1);

            // Ease curve (optional but nicer)
            t = t * t; // ease-in acceleration

            float currentInterval = Mathf.Lerp(startInterval, endInterval, t);

            yield return new WaitForSeconds(currentInterval);
        }

        Debug.Log("All handprints activated!");

        //yield return new WaitForSeconds(1f);

        StopRedLightSound();
        yield return StartCoroutine(FadeOutHandprints(2f));

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

    IEnumerator FadeOutHandprints(float duration)
    {
        Debug.Log("Fading out handprints...");

        float elapsed = 0f;

        Renderer[] renderers = new Renderer[handprintPlanes.Length];
        Material[] materials = new Material[handprintPlanes.Length];

        for (int i = 0; i < handprintPlanes.Length; i++)
        {
            if (handprintPlanes[i] != null)
            {
                renderers[i] = handprintPlanes[i].GetComponent<Renderer>();
                materials[i] = renderers[i].material;
            }
        }

        // Reset alpha to 1
        foreach (Material mat in materials)
        {
            if (mat == null) continue;

            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = 1f;
                mat.SetColor("_BaseColor", c);
            }
            else
            {
                Color c = mat.color;
                c.a = 1f;
                mat.color = c;
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            foreach (Material mat in materials)
            {
                if (mat == null) continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
                else
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }

            yield return null;
        }

        foreach (GameObject handprint in handprintPlanes)
        {
            if (handprint != null)
                handprint.SetActive(false);
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
    void PlayHandSlapSound()
    {
        if (handprintAudioSource != null && handSlapSounds.Length > 0)
        {
            // Pick random clip
            AudioClip clip = handSlapSounds[Random.Range(0, handSlapSounds.Length)];

            handprintAudioSource.pitch = Random.Range(minPitch, maxPitch);
            handprintAudioSource.volume = volume;

            handprintAudioSource.PlayOneShot(clip);
        }
    }

}
