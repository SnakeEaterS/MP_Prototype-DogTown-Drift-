using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public Transform playerBody; // Assign this to the player's transform
    public Shooting shootingScript; // Assign the shooting script to access the crosshair
    
    public float sensitivity = 200f;

    float xRotation = 0f;
    float yRotationOffset = 0f;
    bool regainControl = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        yRotationOffset = 0f;

        // Check if the current scene is "MainGame"
        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            StartCoroutine(TiltCameraUpAndBack(45f, 4.5f));
        }
        else
        {
            shootingScript.enabled = true;
            regainControl = true;
        }
    }

    void Update()
    {
        if (!regainControl)
            return;
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

    public IEnumerator TiltCameraUpAndBack(float tiltAmount = 15f, float duration = 1f)
    {
        float thirdDuration = duration / 3f;
        float timer = 0f;
        float startXRotation = xRotation;
        float targetXRotation = Mathf.Clamp(xRotation - tiltAmount, -90f, 90f);

        // Tilt up
        while (timer < thirdDuration)
        {
            timer += Time.deltaTime;
            xRotation = Mathf.Lerp(startXRotation, targetXRotation, timer / thirdDuration);
            transform.localRotation = Quaternion.Euler(xRotation, yRotationOffset, 0f);
            yield return null;
        }

        timer = 0f;

        while (timer < thirdDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Reset timer for downward motion
        timer = 0f;

        // Tilt back down
        while (timer < thirdDuration)
        {
            timer += Time.deltaTime;
            xRotation = Mathf.Lerp(targetXRotation, startXRotation, timer / thirdDuration);
            transform.localRotation = Quaternion.Euler(xRotation, yRotationOffset, 0f);
            yield return null;
        }

        xRotation = startXRotation;
        regainControl = true;
        shootingScript.enabled = true; // Re-enable shooting script
    }

}