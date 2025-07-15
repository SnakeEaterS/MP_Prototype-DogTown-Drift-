using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleHelicopterFollower : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform player;
    public Vector3 followOffset = new Vector3(0, 5, -10); // Offset from player

    [Header("Helicopter Model & Propellers")]
    public Transform helicopterModel;
    public Transform mainRotor;
    public Transform tailRotor;

    [Header("Propeller Speeds")]
    public float mainRotorSpeed = 1000f;
    public float tailRotorSpeed = 500f;

    [Header("Flight Settings")]
    public float liftForce = 10f;
    public float maxFollowSpeed = 15f;
    public float yawTurnSpeed = 30f;
    public float tiltAmount = 15f; // Visual tilt degrees

    public float movementSmoothness = 2f;

    [Header("Camera")]
    public Transform cameraTransform;
    public Camera pressCamera;

    [Header("Dynamic Zoom")]
    public float minZoom = 30f;
    public float maxZoom = 60f;
    public float zoomChangeInterval = 4f;
    public float zoomLerpSpeed = 2f;

    [Header("Offset Shake")]
    public float offsetShakeAmount = 0.2f;
    public float offsetShakeSpeed = 2f;

    private Rigidbody rb;

    private Vector3 desiredWorldPosition;

    private float targetZoom;
    private float zoomTimer;
    private Vector3 shakeOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        if (pressCamera != null)
        {
            targetZoom = pressCamera.fieldOfView;
            zoomTimer = zoomChangeInterval;
        }
    }

    void Update()
    {
        if (player == null) return;

        // --- Calculate desired world position with offset ---
        desiredWorldPosition = player.position +
                               player.right * followOffset.x +
                               Vector3.up * followOffset.y +
                               player.forward * followOffset.z;

        // --- Spin propellers ---
        if (mainRotor != null)
            mainRotor.Rotate(Vector3.up, mainRotorSpeed * Time.deltaTime, Space.Self);
        if (tailRotor != null)
            tailRotor.Rotate(Vector3.right, tailRotorSpeed * Time.deltaTime, Space.Self);

        // --- Camera zoom timer countdown ---
        if (pressCamera != null)
        {
            zoomTimer -= Time.deltaTime;
            if (zoomTimer <= 0f)
            {
                targetZoom = Random.Range(minZoom, maxZoom);
                zoomTimer = zoomChangeInterval + Random.Range(-1f, 1f);
            }
        }
    }

    void LateUpdate()
    {
        if (player == null || cameraTransform == null) return;

        // --- Dynamic Zoom Lerp ---
        if (pressCamera != null)
        {
            pressCamera.fieldOfView = Mathf.Lerp(pressCamera.fieldOfView, targetZoom, Time.deltaTime * zoomLerpSpeed);
        }

        // --- Generate shake offset ---
        shakeOffset = new Vector3(
            Mathf.PerlinNoise(Time.time * offsetShakeSpeed, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, Time.time * offsetShakeSpeed) - 0.5f,
            0f
        ) * offsetShakeAmount;

        // --- NOTE: Camera position is NOT changed here to keep it fixed ---

        // --- Smoothly rotate camera to look at player upper body ---
        Vector3 lookTarget = player.position + Vector3.up * 1.5f;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - cameraTransform.position);
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, Time.deltaTime * movementSmoothness);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // --- LIFT to hover ---
        Vector3 upForce = Vector3.up * liftForce;
        rb.AddForce(upForce, ForceMode.Acceleration);

        Vector3 toTarget = desiredWorldPosition - transform.position;
        Vector3 horizontalToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
        float distance = horizontalToTarget.magnitude;

        // Dead zone to stop jittering near target
        if (distance < 0.5f) // Adjust threshold as needed
        {
            // Close enough: slow down horizontal movement smoothly
            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0f;

            Vector3 velocityChange = -horizontalVelocity; // reduce velocity to zero
            rb.AddForce(velocityChange * movementSmoothness, ForceMode.VelocityChange);

            // Optional: snap position horizontally exactly to desired target
            transform.position = new Vector3(desiredWorldPosition.x, transform.position.y, desiredWorldPosition.z);
        }
        else
        {
            // Normal follow behavior
            Vector3 desiredVelocity = horizontalToTarget.normalized * Mathf.Clamp(distance, 0f, maxFollowSpeed);
            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0f;

            Vector3 velocityChange = desiredVelocity - horizontalVelocity;
            rb.AddForce(velocityChange * movementSmoothness, ForceMode.VelocityChange);

            // Rotate to face movement direction smoothly
            if (distance > 1f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(horizontalToTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * yawTurnSpeed);
            }
        }

        // --- Visual tilt & sway ---
        if (helicopterModel != null)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
            float forwardSpeed = localVelocity.z;
            float sidewaysSpeed = localVelocity.x;

            float tiltX = Mathf.Clamp(forwardSpeed, -1f, 1f) * tiltAmount;
            float tiltZ = Mathf.Clamp(-sidewaysSpeed, -1f, 1f) * tiltAmount;

            Quaternion targetTilt = Quaternion.Euler(tiltX, 0f, tiltZ);
            helicopterModel.localRotation = Quaternion.Slerp(
                helicopterModel.localRotation,
                targetTilt,
                Time.deltaTime * 3f
            );
        }
    }
}
