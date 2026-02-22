using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusDoors : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorTransform1;           // The door object to move/rotate
    public Transform doorTransform2;           // Optional second door (for double doors)
    public bool isOpen = true;
    public float openSpeed = 2f;

    [Header("Sliding Door Settings")]
    public Vector3 slideOffset1 = new Vector3(0, 0, 1.5f);  // How far door 1 slides
    public Vector3 slideOffset2 = new Vector3(0, 0, -1.5f); // How far door 2 slides

    private Vector3 closedPosition1;
    private Vector3 closedPosition2;
    private Vector3 openPosition1;
    private Vector3 openPosition2;

    void Start()
    {
        // Store closed positions
        if (doorTransform1 != null)
        {
            closedPosition1 = doorTransform1.localPosition;
            openPosition1 = closedPosition1 + slideOffset1;
        }

        if (doorTransform2 != null)
        {
            closedPosition2 = doorTransform2.localPosition;
            openPosition2 = closedPosition2 + slideOffset2;
        }
    }

    void Update()
    {
        // Move door 1
        if (doorTransform1 != null)
        {
            Vector3 targetPosition1 = isOpen ? openPosition1 : closedPosition1;
            doorTransform1.localPosition = Vector3.Lerp(
                doorTransform1.localPosition,
                targetPosition1,
                openSpeed * Time.deltaTime
            );
        }

        // Move door 2
        if (doorTransform2 != null)
        {
            Vector3 targetPosition2 = isOpen ? openPosition2 : closedPosition2;
            doorTransform2.localPosition = Vector3.Lerp(
                doorTransform2.localPosition,
                targetPosition2,
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
        isOpen = !isOpen;
        Debug.Log("Door " + (isOpen ? "opened" : "closed"));
    }

    public void OpenDoor()
    {
        isOpen = true;
        Debug.Log("Door opened");
    }

    public void CloseDoor()
    {
        isOpen = false;
        Debug.Log("Door closed");
    }
}
