using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusDoors : MonoBehaviour
{
    [Header("Door Settings")]
    public BusController busController;           // Reference to the bus controller to check if bus is moving

    [Header("Door Settings")]
    public Transform doorTransform1;           // The door object to move/rotate
    public Transform doorTransform2;           // Optional second door (for double doors)
    public bool isOpen = true;
    public float openSpeed = 2.5f;

    [Header("Rotation Door Settings")]
    public float rotationAngle = 90f;

    private Quaternion closedRotation1;
    private Quaternion closedRotation2;
    private Quaternion openRotation1;
    private Quaternion openRotation2;


    [Header("Door Audio")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenClip;
    public AudioClip doorCloseClip;

    void Start()
    {
        // Store open rotations
        if (doorTransform1 != null)
        {
            openRotation1 = doorTransform1.localRotation;
            closedRotation1 = openRotation1 * Quaternion.Euler(0, -rotationAngle, 0);
        }

        if (doorTransform2 != null)
        {
            openRotation2 = doorTransform2.localRotation;
            closedRotation2 = openRotation2 * Quaternion.Euler(0, -rotationAngle, 0);
        }
    }

    void Update()
    {
        // Rotate door 1
        if (doorTransform1 != null)
        {
            Quaternion targetRotation1 = isOpen ? openRotation1 : closedRotation1;
            doorTransform1.localRotation = Quaternion.Slerp(
                doorTransform1.localRotation,
                targetRotation1,
                openSpeed * Time.deltaTime
            );
        }

        // Rotate door 2
        if (doorTransform2 != null)
        {
            Quaternion targetRotation2 = isOpen ? openRotation2 : closedRotation2;
            doorTransform2.localRotation = Quaternion.Slerp(
                doorTransform2.localRotation,
                targetRotation2,
                openSpeed * Time.deltaTime
            );
        }
    }

    // Called when player clicks the door
    public void Interact()
    {
        ToggleDoor();
    }

    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public void OpenDoor()
    {
        isOpen = true;

        if (busController != null && busController.currentGear != 0 )
        {
            busController.ParkGear();
        }

        PlayDoorSound(doorOpenClip);


        Debug.Log("Door opened, bus Parked.");
    }

    public void CloseDoor()
    {
        isOpen = false;

        PlayDoorSound(doorCloseClip);

        Debug.Log("Door closed");
    }

    void PlayDoorSound(AudioClip clip)
    {
        if (doorAudioSource != null && clip != null)
        {
            doorAudioSource.PlayOneShot(clip);
        }
    }
}
