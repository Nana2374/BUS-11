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

    public static bool doorDisabled = false;


    //[Header("UI")]
    //public GameObject interactUI; // assign your "Door" text here


    public void Interact()
    {
        switch (buttonType)
        {
            case ButtonType.DoorToggle:

                if (doorDisabled)
                {
                    Debug.Log("Door button is disabled!");
                    return;
                }

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

}
//    public void ShowUI()
//    {
//        if (interactUI != null)
//            interactUI.SetActive(true);
//    }


//    public void HideUI()
//    {
//        if (interactUI != null)
//            interactUI.SetActive(false);
//    }
//}

/*void PlayHorn()
{
    if (hornSound != null)
    {
        hornSound.Play();
        Debug.Log("Horn pressed!");
    }
}*/