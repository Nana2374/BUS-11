using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivty = 100f;

    public Transform playerBody;

    public SnaptoSeat seatController; // Add reference to your seat script

    float xRotation = 0f;
    float yRotation = 0f; // Store Y rotation separately

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivty * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivty * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        // ONLY rotate player body if NOT seated
        if (seatController == null || !seatController.isSeated)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
