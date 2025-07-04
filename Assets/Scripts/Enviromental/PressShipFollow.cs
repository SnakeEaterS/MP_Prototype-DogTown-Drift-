using UnityEngine;

public class PressShipFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 baseOffset = new Vector3(5f, 3f, -4f);
    public float followSmoothness = 3f;

    public Transform cameraTransform;
    public Camera pressCamera;

    [Header("Dynamic Zoom")]
    public float minZoom = 30f;
    public float maxZoom = 60f;
    public float zoomChangeInterval = 4f;
    public float zoomLerpSpeed = 2f;

    private float targetZoom;
    private float zoomTimer;

    [Header("Offset Shake")]
    public float offsetShakeAmount = 0.2f;
    public float offsetShakeSpeed = 2f;

    private Vector3 shakeOffset;

    void Start()
    {
        if (pressCamera != null)
        {
            targetZoom = pressCamera.fieldOfView;
            zoomTimer = zoomChangeInterval;
        }
    }

    void LateUpdate()
    {
        if (player == null || cameraTransform == null) return;

        // Random zoom logic
        zoomTimer -= Time.deltaTime;
        if (zoomTimer <= 0f)
        {
            targetZoom = Random.Range(minZoom, maxZoom);
            zoomTimer = zoomChangeInterval + Random.Range(-1f, 1f);
        }

        if (pressCamera != null)
        {
            pressCamera.fieldOfView = Mathf.Lerp(pressCamera.fieldOfView, targetZoom, Time.deltaTime * zoomLerpSpeed);
        }

        // Generate a small random shake offset
        shakeOffset = new Vector3(
            Mathf.PerlinNoise(Time.time * offsetShakeSpeed, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, Time.time * offsetShakeSpeed) - 0.5f,
            0f
        ) * offsetShakeAmount;

        // Final offset (base + shake)
        Vector3 totalOffset = baseOffset + shakeOffset;

        Vector3 desiredPosition = player.position
                                + player.right * totalOffset.x
                                + Vector3.up * totalOffset.y
                                + player.forward * totalOffset.z;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSmoothness);

        cameraTransform.LookAt(player.position + Vector3.up * 1.5f); // Aim at upper body
    }
}
