using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusButton : MonoBehaviour, IInteractable 
{
    [Header("Button Type")]
    public ButtonType buttonType;

    public enum ButtonType
    {
        DoorToggle,
        //Horn,
        // Add more button types as needed
    }

    [Header("References")]
    public BusDoors doorsToControl;           // For door buttons
    public AudioSource hornSound;             // For horn button

    public void Interact()
    {
        switch (buttonType)
        {
            case ButtonType.DoorToggle:
                if (doorsToControl != null)
                {
                    doorsToControl.ToggleDoor();
                    Debug.Log("Door button pressed!");
                }
                break;

            //case ButtonType.Horn:
                //PlayHorn();
                //break;
        }
    }

    /*void PlayHorn()
    {
        if (hornSound != null)
        {
            hornSound.Play();
            Debug.Log("Horn pressed!");
        }
    }*/
}
