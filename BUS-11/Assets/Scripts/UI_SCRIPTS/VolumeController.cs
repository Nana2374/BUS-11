using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Master Volume Slider")]
    public Slider masterVolumeSlider;

    void Start()
    {
        if (masterVolumeSlider != null)
        {
            // Set slider to saved volume (default 0.5)
            float savedVolume = AudioManager.Instance.GetMasterVolume();
            masterVolumeSlider.value = savedVolume;

            // Add listener
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            //Debug.Log($"Slider initialized to: {savedVolume}");
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        //Debug.Log($"Volume changed to: {value}");
    }

    void OnDestroy()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }
    }
}