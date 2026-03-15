using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool enableShake = true;
    public float shakeIntensity = 0.03f;
    public float shakeSpeed = 10f;

    [Header("References")]
    public BusController busController;
    public SnaptoSeat seatController;

    private Vector3 originalLocalPosition;
    private float shakeTimer = 0f;

    void Start()
    {
        originalLocalPosition = transform.localPosition;

        if (busController == null)
        {
            busController = FindObjectOfType<BusController>();
        }
        // Auto-find seat controller
        if (seatController == null)
        {
            seatController = FindObjectOfType<SnaptoSeat>();
        }
    }

    void Update()
    {
        if (!enableShake) return;

        // Get the correct base position (standing or seated)
        Vector3 basePosition = originalLocalPosition;

        if (seatController != null && seatController.isSeated)
        {
            // Use seated position as base when seated
            basePosition = seatController.seatedCameraLocalPos;
        }

        // STOP camera shake during dialogue
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            // smoothly reset camera position
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, 5f * Time.deltaTime);
            return;
        }

        // Only shake if player is NOT driving
        if (busController != null && busController.playerDriving)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, 5f * Time.deltaTime);
            return;
        }

        // Check if any movement keys are pressed
        bool isMoving = Input.GetKey(KeyCode.W) ||
                       Input.GetKey(KeyCode.A) ||
                       Input.GetKey(KeyCode.S) ||
                       Input.GetKey(KeyCode.D);

        if (isMoving)
        {
            // Increment shake timer
            shakeTimer += Time.deltaTime * shakeSpeed;

            // Calculate shake offset using Perlin noise
            //float shakeX = (Mathf.PerlinNoise(shakeTimer, 0f) - 0.5f) * 2f * shakeIntensity;
            float shakeY = (Mathf.PerlinNoise(0f, shakeTimer) - 0.5f) * 2f * shakeIntensity;
            float shakeZ = (Mathf.PerlinNoise(shakeTimer, shakeTimer) - 0.5f) * 2f * shakeIntensity;

            // Apply shake to local position
            transform.localPosition = originalLocalPosition + new Vector3(0, shakeY, shakeZ);

            //Debug.Log("Shaking!");
        }
        else
        {
            // Return to base position (standing or seated)
            transform.localPosition = Vector3.Lerp(transform.localPosition, basePosition, 5f * Time.deltaTime);
        }
    }
}
