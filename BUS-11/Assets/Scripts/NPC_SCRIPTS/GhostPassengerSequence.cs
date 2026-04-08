using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostPassengerSequence : MonoBehaviour
{
    [Header("Horror SFX")]
    public AudioSource horrorSFX;  // Assign in inspector

    [Header("Rotation Settings")]
    public float rotationDuration = 1f; // faster rotation
    public Transform cameraTargetRotation;
    public Transform cameraTransform;

    [Header("Monologue")]
    public DialogueData monologueData;

    [Header("Player Control")]
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public Transform playerTransform;

    [Header("References")]
    public GhostPassenger ghostPassenger;
    public GameManager gameManager;

    [Header("Objects to Destroy")]
    public List<GameObject> objectsToDestroy;

    [Header("Next Passengers")]
    public List<GameObject> nextPassengers; // assign scene objects (inactive)

    private bool sequenceTriggered = false;

    void Update()
    {
        if (!sequenceTriggered &&
            ghostPassenger != null &&
            DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive() &&
            ghostPassenger.HasInteracted())
        {
            sequenceTriggered = true;

            // Stop horror music
            if (DoorDisableZone.Instance != null)
            {
                if (DoorDisableZone.Instance.horrorMusic != null)
                    DoorDisableZone.Instance.horrorMusic.Stop();

                if (DoorDisableZone.Instance.additionalMusic != null)
                    DoorDisableZone.Instance.additionalMusic.Stop();
            }

            StartCoroutine(HandleSequence());
        }
    }

    IEnumerator HandleSequence()
    {
        yield return new WaitForSeconds(0.5f);

        // DESTROY OTHER OBJECTS
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }

        // ENABLE NEW PASSENGERS
        foreach (GameObject obj in nextPassengers)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        //  LOCK PLAYER CONTROLS
        if (playerMovement != null)
            playerMovement.canMove = false;

        if (mouseLook != null)
            mouseLook.canLook = false;

        //  FORCE ROTATION TO Z DIRECTION
        if (playerTransform != null)
        {
            StartCoroutine(RotatePlayerToZ());
        }
    }

    IEnumerator RotatePlayerToZ()
    {
        // Play horror SFX at start
        if (horrorSFX != null)
            horrorSFX.Play();

        // Player body handles Y (yaw)
        Quaternion startBodyRot = playerTransform.rotation;
        Quaternion targetBodyRot = Quaternion.Euler(0f, cameraTargetRotation.eulerAngles.y, 0f);

        // Camera handles X (pitch)
        Quaternion startCamRot = cameraTransform.localRotation;
        float targetPitch = cameraTargetRotation.eulerAngles.x;
        // Convert to -90/90 range so it matches MouseLook's xRotation
        if (targetPitch > 180f) targetPitch -= 360f;
        Quaternion targetCamRot = Quaternion.Euler(targetPitch, 0f, 0f);

        float time = 0f;
        while (time < rotationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / rotationDuration);

            playerTransform.rotation = Quaternion.Slerp(startBodyRot, targetBodyRot, t);
            cameraTransform.localRotation = Quaternion.Slerp(startCamRot, targetCamRot, t);

            yield return null;
        }

        playerTransform.rotation = targetBodyRot;
        cameraTransform.localRotation = targetCamRot;

        // Sync MouseLook so it doesn't snap back when canLook is re-enabled
        if (mouseLook != null)
            mouseLook.SetRotation(targetPitch, cameraTargetRotation.eulerAngles.y);

        yield return new WaitForSeconds(0.5f);

        // Continue with monologue
        if (MonologueManager.Instance != null && monologueData != null)
        {
            MonologueManager.Instance.PlayMonologue(monologueData);
            yield return new WaitUntil(() => !MonologueManager.Instance.IsPlaying);
        }

        // Fade out and load credits
        if (ScreenFader.Instance != null)
            yield return StartCoroutine(ScreenFader.Instance.FadeOut());

        gameManager.ShowEndCredits();
    }
}