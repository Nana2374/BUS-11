using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip walkLoop;

    public void StartWalking()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = walkLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopWalking()
    {
        audioSource.Stop();
    }
}
