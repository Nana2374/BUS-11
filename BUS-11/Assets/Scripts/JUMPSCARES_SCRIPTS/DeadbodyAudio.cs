using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadbodyAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource deadbodyAudioSource;
    public AudioClip bodyFallClip;
    public AudioClip bodyRunOverClip;
    public AudioClip bodyScreamClip;

    private bool hasTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collider OR its parent has the "Bus" tag
        if (collision.collider.CompareTag("Bus") || collision.collider.transform.root.CompareTag("Bus"))
        {
            PlayScreamAudio();
            hasTriggered = true;
            Debug.Log("Bus rolled over body.");
        }
    }

    void PlayScreamAudio()
    {
        if (deadbodyAudioSource != null && bodyRunOverClip != null && !hasTriggered)
        {
            deadbodyAudioSource.volume = 1f;

            deadbodyAudioSource.PlayOneShot(bodyScreamClip);
        }
    }
}
