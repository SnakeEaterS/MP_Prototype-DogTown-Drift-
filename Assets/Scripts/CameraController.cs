using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 200f;
    public Transform playerBody; // Assign this to the player's transform

    float xRotation = 0f;
    float yRotationOffset = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        yRotationOffset = 0f;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Vertical rotation (up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Horizontal rotation relative to player
        yRotationOffset += mouseX;
        yRotationOffset = Mathf.Clamp(yRotationOffset, -90f, 90f); // Clamp relative to player

        // Apply camera rotation
        transform.localRotation = Quaternion.Euler(xRotation, yRotationOffset, 0f);

    }
}