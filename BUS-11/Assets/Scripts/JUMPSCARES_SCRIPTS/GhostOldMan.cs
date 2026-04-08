using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostOldMan : MonoBehaviour
{
    public PassengerController passengerController;
    public DialogueData monologueData;

    [Header("Audio")]
    public AudioSource oldmanAudioSource;
    public AudioSource footstepsAudioSource;
    public AudioClip laughClip;
    public AudioClip breathingClip;
    public AudioClip wetFootstepsClip;

    private bool hasTriggered = false;
    private bool isWalking = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (passengerController.CurrentState == PassengerController.PassengerState.WalkingToEntry || passengerController.CurrentState == PassengerController.PassengerState.WalkingToSeat)
        {
            isWalking = true;

            PlayWetFootstepsAudio();

            //Debug.Log("Wet footsteps.");
        }
        else
        {
            isWalking = false;

            StopWetFootstepsAudio();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider OR its parent has the "Bus" tag
        if (other.CompareTag("Bus") || other.transform.root.CompareTag("Bus"))
        {
            StartCoroutine(PlayLaughAudio());
            hasTriggered = true;
            //Debug.Log("Laughing audio.");
        }
    }

    IEnumerator PlayLaughAudio()
    {
        if (oldmanAudioSource != null && laughClip != null && !hasTriggered)
        {
            oldmanAudioSource.volume = 1f;

            oldmanAudioSource.PlayOneShot(laughClip);

            yield return new WaitForSeconds(0.5f);

            MonologueManager.Instance.PlayMonologue(monologueData);
        }
    }

    void PlayWetFootstepsAudio()
    {
        if (footstepsAudioSource != null && wetFootstepsClip != null)
        {
            if (isWalking)
            {
                // Only start if not already playing this clip
                if (!footstepsAudioSource.isPlaying || footstepsAudioSource.clip != wetFootstepsClip)
                {
                    footstepsAudioSource.clip = wetFootstepsClip;
                    footstepsAudioSource.loop = true;
                    footstepsAudioSource.volume = 1f;
                    footstepsAudioSource.Play();
                }
            }
            else
            {
                StopWetFootstepsAudio();
            }
        }
    }

    void StopWetFootstepsAudio()
    {
        if (footstepsAudioSource != null && footstepsAudioSource.isPlaying)
        {
            footstepsAudioSource.loop = false;
            footstepsAudioSource.Stop();
        }
    }
}
