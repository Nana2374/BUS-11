using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodyHandprints : MonoBehaviour
{
    [Header("References")]
    public PassengerController ghostLady; // Drag the ghost lady passenger here
    public GameObject[] handprintPlanes; // Drag all 20 handprint planes here
    public GameObject[] preFlashPlanes;
    public GameObject facePlane;

    [Header("Activation Settings")]
    public float activationInterval = 0.5f; // Time between each handprint appearing
    public float delayBeforeStart = 2f; // Delay before first handprint
    public float flashDuration = 0.2f;  // How long each pre-flash plane is visible
    public float flashInterval = 1f;

    [Header("Timing Curve")]
    public float startInterval = 0.5f;   // slow at start
    public float endInterval = 0.05f;     // very fast near end

    [Header("Light Flicker")]
    public Light[] lightsToFlicker; // Assign bus interior lights, headlights, etc.
    public float flickerDuration = 10f; // How long lights flicker
    public float flickerSpeed = 0.1f; // How fast they flicker (lower = faster)

    [Header("Audio")]
    public AudioClip[] handSlapSounds;
    public AudioSource handprintAudioSource;
    public AudioSource lightSource;     // Drag your AudioSource here
    public AudioClip lightSFX;       // Drag your sound clip here
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    public float volume = 1f;

    public AudioClip screamClip;
    public AudioSource screamAudioSource;

    private bool hasTriggered = false;
    private float[] originalIntensities;

    void Start()
    {
        // Hide all handprints at start
        foreach (GameObject handprint in handprintPlanes)
        {
            if (handprint != null)
            {
                handprint.SetActive(false);
            }
        }
        if (preFlashPlanes != null)
        {
            foreach (GameObject flashPlane in preFlashPlanes)
            {
                if (flashPlane != null)
                {
                    flashPlane.SetActive(false);
                }
            }
        }
        if (facePlane != null)
        {
            facePlane.SetActive(false);
        }

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

        // Handprint audio source
        if (handprintAudioSource == null)
        {
            handprintAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Scream audio source (attach to ghost lady for positioning)
        if (screamAudioSource == null && ghostLady != null)
        {
            screamAudioSource = ghostLady.gameObject.AddComponent<AudioSource>();
        }

        // Register both
        AudioManager.Instance.RegisterSFXSource(handprintAudioSource);
        AudioManager.Instance.RegisterSFXSource(screamAudioSource);
        AudioManager.Instance.RegisterSFXSource(lightSource);
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
                StartCoroutine(ActivateHandprints());
                Coroutine flickerCoroutine = StartCoroutine(FlickerLights(5f));
                PlayRedLightSound();
            }
            else
            {
                Debug.Log("Ghost lady is safe on the bus.");
                Destroy(gameObject);
            }
        }
    }

    IEnumerator ActivateHandprints()
    {
        // Wait before starting
        yield return new WaitForSeconds(delayBeforeStart);

        int total = handprintPlanes.Length;

        StartCoroutine(StagedFaceJumpscare());

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

        yield return StartCoroutine(FaceJumpscare(0.5f, 8f, 5f, 3));

        //yield return new WaitForSeconds(1f);

        StopRedLightSound();
        yield return StartCoroutine(FadeOutHandprints(2f));

        Destroy(gameObject); 
    }

    IEnumerator StagedFaceJumpscare()
    {
        int total = preFlashPlanes.Length;
        // 1 Flash planes
        for (int i = 0; i < total; i++)
        {
            GameObject flashPlane = preFlashPlanes[i];
            if (flashPlane == null) continue;

            flashPlane.SetActive(true);
            yield return new WaitForSeconds(flashDuration);
            flashPlane.SetActive(false);
            yield return new WaitForSeconds(flashInterval);
        }
    }

    IEnumerator FaceJumpscare(float duration, float moveSpeed, float rotationAmount = 5f, int twitchCount = 3)
    {
        PlayScream();

        if (facePlane == null) yield break;

        float elapsed = 0f;

        Transform face = facePlane.transform;

        // Store original rotation
        Quaternion originalRot = face.rotation;

        // Each twitch lasts this long
        float twitchDuration = duration / twitchCount;

        // Optional: ensure it's visible
        facePlane.SetActive(true);

        for (int i = 0; i < twitchCount; i++)
        {
            float twitchElapsed = 0f;
            // Pick random rotation direction for this twitch
            Vector3 twitchAxis = new Vector3(
                Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f)
            ).normalized;

            Quaternion targetRot = originalRot * Quaternion.Euler(twitchAxis * rotationAmount);

            while (twitchElapsed < twitchDuration)
            {
                twitchElapsed += Time.deltaTime;
                float t = twitchElapsed / twitchDuration;

                // Move toward player
                Vector3 direction = (Camera.main.transform.position - face.position).normalized;
                face.position += direction * moveSpeed * Time.deltaTime;

                // Lerp rotation for this twitch
                face.rotation = Quaternion.Slerp(originalRot, targetRot, Mathf.PingPong(t * 2f, 1f));

                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);

        facePlane.SetActive(false);

        Destroy(facePlane);
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

    void PlayScream()
    {
        if (screamAudioSource != null && screamClip != null)
        {
            screamAudioSource.pitch = Random.Range(0.9f, 1.1f);
            screamAudioSource.volume = 1f;

            screamAudioSource.PlayOneShot(screamClip);
        }
    }

    IEnumerator FlickerLights(float duration)
    {
        if (lightsToFlicker == null || lightsToFlicker.Length == 0) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
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

    /*void CleanupHandprints()
    {
        foreach (GameObject handprint in handprintPlanes)
        {
            if (handprint != null)
                handprint.SetActive(false);
        }
    }*/

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

    void PlayRedLightSound()
    {
        if (lightSource != null && lightSFX != null)
        {
            lightSource.clip = lightSFX;
            lightSource.loop = true;
            lightSource.Play();
        }
    }
    void StopRedLightSound()
    {
        if (lightSource != null && lightSource.isPlaying)
        {
            lightSource.Stop();
        }
    }

    void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterSFXSource(handprintAudioSource);
            AudioManager.Instance.UnregisterSFXSource(screamAudioSource);
        }
    }
}
