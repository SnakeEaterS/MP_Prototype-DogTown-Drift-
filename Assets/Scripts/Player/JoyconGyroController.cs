using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class JoyconRevController : MonoBehaviour
{
    private List<Joycon> joycons;
    private Joycon j;

    public float baseSpeed = 15f;
    public float speed = 0f;
    private bool isRumbling = false;

    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;
    public BikeSplineFollower splineFollower;

    [Header("Freeform Turning")]
    public float maxHorizontalOffset = 15f;
    public float turnSpeed = 20f;
    private float currentOffset = 0f;

    [Header("Turbo Settings")]
    public float turboDuration = 3f;
    public float turboChargeRate = 30f;
    public float turboCooldown = 2f;
    public float turboThreshold = 100f;
    public float turboSpeed = 60f;

    private float turboCharge = 0f;
    private float turboTimer = 0f;
    private float turboCooldownTimer = 0f;
    private bool isInTurbo = false;

    private PlayerHealth playerHealth;
    public TextMeshProUGUI speedText;

    [Header("Twist Smoothing")]
    public float twistSmoothing = 10f;

    private float rawTwist = 0f;
    private float smoothedTwistAngle = 0f;

    void Start()
    {
        joycons = JoyconManager.Instance.j;

        if (joycons.Count >= 1)
        {
            j = joycons[0];
            Debug.Log("Joy-Con connected.");
        }
        else
        {
            Debug.LogWarning("No Joy-Cons connected. Gyro features will be disabled.");
        }

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth script not found.");
        }

        speed = baseSpeed;
    }

    void Update()
    {
        DetectAndDebugTwist();
        CheckTurboActivationTrigger();
        AutoChargeTurbo();
        HandleTurboState();

        if (!isInTurbo)
            speed = baseSpeed;

        HandleTurningInput();
        UpdateUI();

        if (splineFollower != null)
            splineFollower.speed = speed;
    }

    private void DetectAndDebugTwist()
    {
        if (j == null) return;

        Quaternion currentRotation = j.GetVector();
        float currentZ = currentRotation.eulerAngles.z;

        // Convert 0-360 to -180 to 180
        if (currentZ > 180f) currentZ -= 360f;

        rawTwist = currentZ;
        smoothedTwistAngle = Mathf.Lerp(smoothedTwistAngle, rawTwist, Time.deltaTime * twistSmoothing);

        Debug.Log($"[Twist Debug] Signed Z: {rawTwist:F1}°");
    }

    private void CheckTurboActivationTrigger()
    {
        if (isInTurbo || turboCooldownTimer > 0f || turboCharge < turboThreshold) return;

        bool turboTriggered = false;

        if (j != null)
        {
            // If Joy-Con connected, use twist logic
            if (rawTwist < 0f)
            {
                turboTriggered = true;
            }
        }
        else
        {
            // If Joy-Con is not connected, allow W key fallback
            if (Input.GetKeyDown(KeyCode.W))
            {
                turboTriggered = true;
            }
        }

        if (turboTriggered)
        {
            ActivateTurbo();
        }
    }

    private void AutoChargeTurbo()
    {
        if (isInTurbo || turboCooldownTimer > 0f) return;

        turboCharge += turboChargeRate * Time.deltaTime;
        turboCharge = Mathf.Clamp(turboCharge, 0, turboThreshold);
    }

    private void ActivateTurbo()
    {
        isInTurbo = true;
        turboTimer = turboDuration;
        turboCharge = 0f;
        speed = turboSpeed;

        if (playerHealth != null)
            playerHealth.IsInvulnerable = true;

        j?.SetRumble(250, 600, 0.3f);
        isRumbling = true;

        StartCoroutine(FOVKick());

        Debug.Log("TURBO ACTIVATED!");
    }

    private void HandleTurboState()
    {
        if (isInTurbo)
        {
            turboTimer -= Time.deltaTime;

            if (turboTimer <= 0f)
            {
                isInTurbo = false;
                turboCooldownTimer = turboCooldown;
                speed = baseSpeed;

                if (playerHealth != null)
                    playerHealth.IsInvulnerable = false;

                j?.SetRumble(0, 0, 0);
                isRumbling = false;

                Debug.Log("Turbo ended.");
            }
        }
        else if (turboCooldownTimer > 0f)
        {
            turboCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleTurningInput()
    {
        float input = 0f;

        if (j != null)
        {
            float stickY = j.GetStick()[1];
            input += stickY;
        }

        if (Input.GetKey(KeyCode.D)) input += 1f;
        if (Input.GetKey(KeyCode.A)) input -= 1f;

        currentOffset += input * turnSpeed * Time.deltaTime;
        currentOffset = Mathf.Clamp(currentOffset, -maxHorizontalOffset, maxHorizontalOffset);

        if (splineFollower != null)
            splineFollower.SetHorizontalOffset(currentOffset);
    }

    private void UpdateUI()
    {
        if (speedText != null)
        {
            speedText.text = $"Speed: {speed * 10f:F1} km/h";
        }
    }

    public float GetTurboChargeNormalized()
    {
        return turboCharge / turboThreshold;
    }

    IEnumerator FOVKick()
    {
        float startFOV = Camera.main.fieldOfView;
        float targetFOV = 80f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            Camera.main.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }

        yield return new WaitForSeconds(turboDuration);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            Camera.main.fieldOfView = Mathf.Lerp(targetFOV, startFOV, t);
            yield return null;
        }
    }
}
