using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusResetController : MonoBehaviour
{
    public Rigidbody rb;
    public BusController busController;

    [Header("Reset Monologues")]
    public DialogueData firstResetMonologue;    // first teleport dialogue
    public DialogueData repeatResetMonologue;   // second time onwards dialogue

    private bool isResetting = false;
    private int resetCount = 0;

    public IEnumerator ResetBusRoutine(Transform resetPoint)
    {
        if (isResetting) yield break;
        if (resetPoint == null || rb == null) yield break;

        isResetting = true;

        // Fade to black first
        if (ScreenFader.Instance != null)
            yield return StartCoroutine(ScreenFader.Instance.FadeOut());

        // Put bus in park
        if (busController != null)
            busController.currentGear = 0;

        // Stop wheel movement
        if (busController != null)
        {
            busController.frontLeft.motorTorque = 0f;
            busController.frontRight.motorTorque = 0f;
            busController.rearLeft.motorTorque = 0f;
            busController.rearRight.motorTorque = 0f;

            busController.frontLeft.brakeTorque = 0f;
            busController.frontRight.brakeTorque = 0f;
            busController.rearLeft.brakeTorque = 0f;
            busController.rearRight.brakeTorque = 0f;
        }

        // Clear physics
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();

        // Teleport rigidbody
        rb.position = resetPoint.position;
        rb.rotation = resetPoint.rotation;

        // Sync transform too
        transform.position = resetPoint.position;
        transform.rotation = resetPoint.rotation;

        Physics.SyncTransforms();

        // Freeze again for park
        rb.constraints = RigidbodyConstraints.FreezeAll;

        Debug.Log("Bus reset to stop start.");

        // Small pause while screen is black
        yield return new WaitForSeconds(0.2f);

        // Fade back in
        if (ScreenFader.Instance != null)
            yield return StartCoroutine(ScreenFader.Instance.FadeIn());

        // Increase reset count
        resetCount++;

        // Play different monologue depending on how many times reset happened
        if (DialogueManager.Instance != null)
        {
            if (resetCount == 1 && firstResetMonologue != null)
            {
                DialogueManager.Instance.StartDialogue(firstResetMonologue);
            }
            else if (resetCount >= 2 && repeatResetMonologue != null)
            {
                DialogueManager.Instance.StartDialogue(repeatResetMonologue);
            }
        }

        isResetting = false;
    }
}

