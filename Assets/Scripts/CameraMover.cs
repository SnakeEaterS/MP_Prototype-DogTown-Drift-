using UnityEngine;

public class FreeRoamCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float boostMultiplier = 2f;

    [Header("Look Settings")]
    public float lookSensitivity = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        // Lock cursor for free look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ======= MOVE =======
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down arrows
        float moveY = 0f;

        if (Input.GetKey(KeyCode.E)) moveY += 1f;  // Up
        if (Input.GetKey(KeyCode.Q)) moveY -= 1f;  // Down

        Vector3 move = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;

        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= boostMultiplier; // Hold shift to move faster
        }

        transform.position += move * speed * Time.deltaTime;

        // ======= LOOK =======
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Unlock cursor on Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
