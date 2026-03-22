using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public BusController busController;

    Vector3 velocity;
    bool isGrounded;

    [Header("Animation")]
    public Animator animator;

    [Header("Seat Control")]
    public SnaptoSeat seatController;

    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip footstepLoop;

    void Start()
    {
        // Register this AudioSource with AudioManager
        /*AudioSource myAudio = GetComponent<footstepSource>();
        if (myAudio != null)
        {
            AudioManager.Instance.RegisterSFXSource(myAudio);
        }*/

        if (footstepSource != null)
        {
            AudioManager.Instance.RegisterSFXSource(footstepSource);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Stop player movement during dialogue
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        // ONLY set walking/idle animations if NOT seated
        if (seatController == null || !seatController.isSeated)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                SetAnimation(true, false); // Walking
            }
            else
            {
                SetAnimation(false, false); // Idle
            }
        }

        bool isMoving = (x != 0 || z != 0);

        if (isMoving && isGrounded && !busController.playerDriving)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.clip = footstepLoop;
                footstepSource.loop = true;
                footstepSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f); // slight variation
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }

    // Helper function to set animations
    void SetAnimation(bool walking, bool sitting)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isSitting", sitting);
    }
}
