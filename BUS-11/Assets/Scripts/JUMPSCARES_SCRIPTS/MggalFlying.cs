using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MggalFlying : MonoBehaviour
{
    [Header("References")]
    public Transform mggal;
    public Rigidbody mggalRigidbody;
    public Transform windscreenTarget;      // Assign the windscreen transform in Inspector
    public float activationSpeed = 10f;     // Min bus speed (km/h) to trigger

    [Header("Flight Settings")]
    public float launchAngle = 30f;         // Lower = flatter arc toward windscreen
    public float disappearDelay = 0.2f;     // How long after impact before disappearing

    [Header("Audio")]
    public AudioSource mggalAudioSource;
    public AudioClip wingsClip;
    public AudioClip thudClip;

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

        if (currentState == GhostState.Waiting)
            CheckBusSpeed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bus") || other.transform.root.CompareTag("Bus"))
        {
            busInTrigger = true;
            // Walk up to the root to get the Rigidbody on the bus
            busRigidbody = other.transform.root.GetComponent<Rigidbody>();
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

    void CheckBusSpeed()
    {
        if (busRigidbody == null) return;

        float busSpeed = busRigidbody.velocity.magnitude * 3.6f; // m/s → km/h
        if (busSpeed >= activationSpeed)
            LaunchGhost();
    }

    void LaunchGhost()
    {
        hasLaunched = true;
        currentState = GhostState.Flying;

        mggal.gameObject.SetActive(true);

        // Enable physics
        mggalRigidbody.isKinematic = false;
        mggalRigidbody.useGravity = true;

        // Play wings audio
        if (mggalAudioSource != null && wingsClip != null)
        {
            mggalAudioSource.volume = 1f;
            mggalAudioSource.PlayOneShot(wingsClip);
        }

        // Calculate ballistic velocity toward windscreen
        Vector3 target = windscreenTarget != null
            ? windscreenTarget.position
            : busRigidbody.transform.position;

        Vector3 direction = (target - mggal.position).normalized;
        float distance = Vector3.Distance(mggal.position, target);
        float angleRad = launchAngle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y);
        float speed = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * angleRad));

        Vector3 launchVelocity = direction * speed * Mathf.Cos(angleRad);
        launchVelocity.y = speed * Mathf.Sin(angleRad);

        mggalRigidbody.velocity = launchVelocity;

        // Face the direction of travel
        mggal.rotation = Quaternion.LookRotation(direction);

        Debug.Log("Ghost launched toward windscreen!");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState != GhostState.Flying) return;

        if (collision.collider.CompareTag("Bus") || collision.transform.root.CompareTag("Bus"))
        {
            currentState = GhostState.Impacted;

            // Stop movement
            mggalRigidbody.velocity = Vector3.zero;
            mggalRigidbody.isKinematic = true;

            // Play thud
            if (mggalAudioSource != null && thudClip != null)
            {
                mggalAudioSource.volume = 1f;
                mggalAudioSource.PlayOneShot(thudClip);
            }

            Debug.Log("Ghost hit windscreen!");
            Invoke(nameof(DisappearGhost), disappearDelay);
        }
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
