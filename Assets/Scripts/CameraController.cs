using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 200f;
    float xRotation = 0f;
    float yRotation = 0f;

    void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Accumulate vertical rotation (up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp vertical rotation

        // Accumulate horizontal rotation (left/right)
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -90f, 90f); // ?? Clamp to ±90° for 180° range

        // Apply rotation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
