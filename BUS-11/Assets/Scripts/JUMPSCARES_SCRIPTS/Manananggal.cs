using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manananggal : MonoBehaviour
{
    [Header("References")]
    public GameObject jumpscareCanvas;   // The Canvas GameObject
    public Animator mggalAnimator;       // Animator on GhostFaceImage

    [Header("Audio")]
    public AudioSource mggalAudioSource;
    public AudioSource jumpscareAudioSource;
    public AudioClip mggalClip;
    public AudioClip whispersClip;
    public AudioClip jumpscareClip;

    public float jumpscareDuration = 1f;
    private bool hasTriggered = false;
    private bool isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider OR its parent has the "Bus" tag
        if (other.CompareTag("Bus") || other.transform.root.CompareTag("Bus"))
        {
            PlayMggalAudio();
            hasTriggered = true;

            StartCoroutine(PlayJumpscare());
        }
    }

    void PlayMggalAudio()
    {
        if (mggalAudioSource != null && mggalClip != null && !hasTriggered)
        {
            mggalAudioSource.volume = 1f;

            mggalAudioSource.PlayOneShot(mggalClip);
            //mggalAudioSource.PlayOneShot(whispersClip);
            //Debug.Log("I see you...");
        }
    }

    IEnumerator PlayJumpscare()
    {
        isPlaying = true;

        yield return new WaitForSeconds(6f);

        // 1. Show the canvas
        jumpscareCanvas.SetActive(true);

        // 3. Play animation + sound
        mggalAnimator.Play("Manananggal Jumpscare", 0, 0f); // Force restart
        PlayScream();

        // 4. Wait for jumpscare duration
        yield return new WaitForSeconds(jumpscareDuration);

        // 6. Hide canvas
        jumpscareCanvas.SetActive(false);
        isPlaying = false;
    }

    void PlayScream()
    {
        if (jumpscareAudioSource != null && jumpscareClip != null)
        {
            jumpscareAudioSource.volume = 1f;

            jumpscareAudioSource.PlayOneShot(jumpscareClip);
            //mggalAudioSource.PlayOneShot(whispersClip);
            //Debug.Log("I see you...");
        }
    }
}
