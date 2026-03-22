using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Master Volume Slider")]
    public Slider masterVolumeSlider;

    [Header("Mouse Sensitivity Slider")]
    public Slider sensitivitySlider;
    public MouseLook mouseLook;

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

        if (sensitivitySlider != null)
        {
            // Auto-find MouseLook if not assigned
            if (mouseLook == null)
            {
                mouseLook = FindObjectOfType<MouseLook>();
            }

            if (mouseLook != null)
            {
                float savedSensitivity = mouseLook.GetSensitivity();
                sensitivitySlider.value = savedSensitivity;
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

                Debug.Log($"Sensitivity slider initialized to: {savedSensitivity}");
            }
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        //Debug.Log($"Volume changed to: {value}");
    }

    public void OnSensitivityChanged(float value)
    {
        if (mouseLook != null)
        {
            mouseLook.SetSensitivity(value);
            Debug.Log($"Sensitivity changed to: {value}");
        }
    }

    void OnDestroy()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }
    }
}