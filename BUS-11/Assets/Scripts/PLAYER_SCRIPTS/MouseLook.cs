using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public bool canLook = true;

    public float mouseSensitivity = 150f;

    public Transform playerBody;

    //public SnaptoSeat seatController; // Add reference to your seat script

    float xRotation = 0f;
    //float yRotation = 0f; // Store Y rotation separately

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        LoadSensitivity();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        // ONLY rotate player body if NOT seated
        /*/if (seatController == null || !seatController.isSeated)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }*/
    }

    void LoadSensitivity()
    {
        // Load saved sensitivity, default to 100 if not saved
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 100f);
        Debug.Log($"Loaded mouse sensitivity: {mouseSensitivity}");
    }

    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
        //Debug.Log($"Mouse sensitivity set to: {sensitivity}");
    }

    public float GetSensitivity()
    {
        return mouseSensitivity;
    }

    public void SetRotation(float xRot, float yRot)
    {
        xRotation = xRot;
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.rotation = Quaternion.Euler(0f, yRot, 0f);
    }
}
