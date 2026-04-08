using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDisableZone : MonoBehaviour
{

    [Header("Objects to Destroy")]
    public List<GameObject> objectsToDestroy;

    [Header("Objects to Enable")]
    public List<GameObject> objectsToEnable;

    [Header("Flicker Lights")]
    public List<Light> flickerLights;        // Lights to flicker
    public AudioSource flickerSFX;           // Sound effect to play
    public float flickerDuration = 3f;       // Total flicker duration
    public float flickerInterval = 0.1f;     // Time between on/off toggles

    private bool zoneTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!zoneTriggered && other.CompareTag("Bus"))
        {
            zoneTriggered = true;
            Debug.Log("ENTERED DOOR DISABLE ZONE");

            // Disable doors
            BusButton.doorDisabled = true;

            // Destroy objects
            foreach (GameObject obj in objectsToDestroy)
            {
                if (obj != null) Destroy(obj);
            }

            // Enable scene objects
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null) obj.SetActive(true);
            }

            // Start light flicker coroutine
            if (flickerLights.Count > 0)
            {
                StartCoroutine(FlickerLightsRoutine());
            }

            // Play SFX with loop enabled
            if (flickerSFX != null)
            {
                flickerSFX.loop = true;
                flickerSFX.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bus"))
        {
            Debug.Log("EXITED DOOR DISABLE ZONE");
            BusButton.doorDisabled = false;
        }
    }

    private IEnumerator FlickerLightsRoutine()
    {
        float timer = 0f;

        while (timer < flickerDuration)
        {
            foreach (Light l in flickerLights)
            {
                if (l != null)
                    l.enabled = !l.enabled; // toggle light
            }

            timer += flickerInterval;
            yield return new WaitForSeconds(flickerInterval);
        }

        // Ensure lights stay on at the end
        foreach (Light l in flickerLights)
        {
            if (l != null)
                l.enabled = true;
        }

        // Stop SFX when flicker ends
        if (flickerSFX != null && flickerSFX.isPlaying)
        {
            flickerSFX.Stop();
        }
    }
}