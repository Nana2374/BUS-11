using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MggalFlying : MonoBehaviour
{
    [Header("References")]
    public Transform mggal;
    public Rigidbody mggalRigidbody;
    public Transform windscreenTarget;      // Assign the windscreen transform in Inspector
    public DialogueData monologueData;

    [Header("Flight Settings")]
    public float disappearDelay = 0.2f;     // How long after impact before disappearing

    [Header("Audio")]
    public AudioSource mggalAudioSource;
    public AudioSource jumpscareAudioSource;
    public AudioClip wingsClip;
    public AudioClip thudClip;
    public AudioClip buzzClip;

    private enum GhostState { Waiting, Flying, Impacted }
    private GhostState currentState = GhostState.Waiting;

    private Rigidbody busRigidbody;
    private bool busInTrigger = false;
    private bool hasLaunched = false;

    void Start()
    {
        mggalRigidbody.isKinematic = true;
        mggalRigidbody.useGravity = false;
        mggal.gameObject.SetActive(false);
    }

    void Update()
    {
        if (hasLaunched || !busInTrigger) return;

        //if (currentState == GhostState.Waiting)
            //CheckBusSpeed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bus") || other.transform.root.CompareTag("Bus"))
        {
            busInTrigger = true;
            // Walk up to the root to get the Rigidbody on the bus
            busRigidbody = other.transform.root.GetComponent<Rigidbody>();

            if (!hasLaunched)
            {
                LaunchGhost();
            }

            Debug.Log("Bus entered ghost trigger.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bus") || other.transform.root.CompareTag("Bus"))
        {
            busInTrigger = false;
            Debug.Log("Bus left ghost trigger.");
        }
    }

    /*void CheckBusSpeed()
    {
        if (busRigidbody == null) return;

        float busSpeed = busRigidbody.velocity.magnitude * 3.6f; // m/s → km/h
        if (busSpeed >= activationSpeed)
            LaunchGhost();
    }*/

    void LaunchGhost()
    {
        hasLaunched = true;
        mggalRigidbody.isKinematic = true;  // Keep kinematic, no physics needed
        mggalRigidbody.useGravity = false;

        StartCoroutine(FlyToWindscreen());
    }

    IEnumerator FlyToWindscreen()
    {
        mggal.gameObject.SetActive(true);
        currentState = GhostState.Flying;

        // Play wings audio
        if (mggalAudioSource != null && wingsClip != null)
        {
            mggalAudioSource.clip = wingsClip;
            mggalAudioSource.loop = true;
            mggalAudioSource.Play();
        }

        if (buzzClip != null)
            jumpscareAudioSource.PlayOneShot(buzzClip);

        Vector3 startPos = mggal.position;
        Vector3 endPos = windscreenTarget.position;
        float duration = 0.5f; // Tune this
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration); // Smooth ease in/out

            // Read windscreenTarget.position live every frame so it tracks the moving bus
            mggal.position = Vector3.Lerp(startPos, windscreenTarget.position, t);
            Quaternion lookRot = Quaternion.LookRotation((windscreenTarget.position - mggal.position).normalized);
            mggal.rotation = lookRot * Quaternion.Euler(20f, 0f, 0f);

            yield return null;
        }

        mggal.position = endPos;

        // Stop wings, play thud
        mggalAudioSource.loop = false;
        mggalAudioSource.Stop();

        if (thudClip != null)
            mggalAudioSource.PlayOneShot(thudClip);

        currentState = GhostState.Impacted;

        yield return new WaitForSeconds(disappearDelay);
        DisappearGhost();

        yield return new WaitForSeconds(0.5f);

        MonologueManager.Instance.PlayMonologue(monologueData);
    }

    void DisappearGhost()
    {
        mggal.gameObject.SetActive(false);
        Debug.Log("Ghost disappeared.");
    }

    void OnDrawGizmos()
    {
        if (windscreenTarget != null && mggal != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(windscreenTarget.position, 0.3f);
            Gizmos.DrawLine(mggal.position, windscreenTarget.position);
        }
    }
}
