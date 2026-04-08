using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPassengerSequence : MonoBehaviour
{
    [Header("Player Control")]
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public Transform playerTransform;

    [Header("References")]
    public GhostPassenger ghostPassenger;

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
        Quaternion startRot = playerTransform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(Vector3.forward);

        float duration = 2f; //  control speed (bigger = slower)
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            playerTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        // Ensure exact final rotation
        playerTransform.rotation = targetRot;
    }
}