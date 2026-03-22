using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip menuMusic;
    public AudioClip gameMusic;

    public float fadeDuration = 1.5f;

    Coroutine currentFade;

    // Track all game AudioSources
    private List<AudioSource> registeredSFXSources = new List<AudioSource>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMenuMusic()
    {
        StartFade(menuMusic);
    }

    public void PlayGameMusic()
    {
        StartFade(gameMusic);
    }

    void StartFade(AudioClip newClip)
    {
        if (musicSource.clip == newClip) return;

        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeMusic(newClip));
    }

    IEnumerator FadeMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;

        // Fade OUT
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Switch track
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade IN
        while (musicSource.volume < startVolume)
        {
            musicSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.volume = startVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PauseSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Pause();
        }

        foreach (AudioSource source in registeredSFXSources)
        {
            if (source != null)
            {
                source.Pause();
            }
        }
    }

    public void ResumeSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.UnPause();
        }

        foreach (AudioSource source in registeredSFXSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
    }

    public void StopSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }

        foreach (AudioSource source in registeredSFXSources)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }

    void LoadVolumes()
    {
        // Load saved volumes, default to 0.5 if not saved
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0.5f);

        musicSource.volume = masterVol;
        sfxSource.volume = masterVol;
    }

    public void SetMasterVolume(float volume)
    {
        // Set music volume
        musicSource.volume = volume;

        // Set AudioManager's SFX volume
        sfxSource.volume = volume;

        // Set ALL registered AudioSources
        foreach (AudioSource source in registeredSFXSources)
        {
            if (source != null)
            {
                source.volume = volume;
            }
        }

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        //Debug.Log($"Set master volume to: {volume} (affected {registeredSFXSources.Count} sources)");
    }

    // Register an AudioSource to be controlled by master volume
    public void RegisterSFXSource(AudioSource source)
    {
        if (source != null && !registeredSFXSources.Contains(source))
        {
            registeredSFXSources.Add(source);
            source.volume = sfxSource.volume; // Set to current master volume
            //Debug.Log($"Registered SFX source: {source.gameObject.name}");
        }
    }

    // Unregister when destroyed
    public void UnregisterSFXSource(AudioSource source)
    {
        registeredSFXSources.Remove(source);
    }

    public float GetMasterVolume()
    {
        return musicSource.volume;
    }

    // Keep these for backward compatibility
    public void SetMusicVolume(float volume)
    {
        SetMasterVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetMasterVolume(volume);
    }

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }
}