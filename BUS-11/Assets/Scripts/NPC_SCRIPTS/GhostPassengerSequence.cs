using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPassengerSequence : MonoBehaviour
{
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

        //DESTROY OTHER OBJECTS (NOT the ghost)
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
    }
}