using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;

public class JoyconRevController : MonoBehaviour
{
    private List<Joycon> joycons;
    private Joycon j;

    public float baseSpeed = 15f;
    public float speed = 0f;
    private bool isRumbling = false;

    private Quaternion initialRotation;
    private bool calibrated = false;

    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;
    public BikeSplineFollower splineFollower;

    [Header("Freeform Turning")]
    public float maxHorizontalOffset = 15f;  // Half of road width (30 units total)
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

    void Start()
    {
        joycons = JoyconManager.Instance.j;

        if (joycons.Count >= 1)
        {
            j = joycons[0];
            Calibrate();
        }
        else
        {
            Debug.LogWarning("No Joy-Cons connected. Gyro features will be disabled.");
        }

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth script not found on the same GameObject.");
        }

        speed = baseSpeed;
    }

    void Calibrate()
    {
        if (j == null) return;

        initialRotation = j.GetVector();
        calibrated = true;
        Debug.Log("Calibrated orientation.");
    }

    void Update()
    {
        AutoChargeTurbo();
        CheckTurboActivationInput();
        HandleTurboState();

        if (!isInTurbo)
        {
            speed = baseSpeed;
        }

        HandleTurningInput();
        UpdateUI();

        if (splineFollower != null)
        {
            splineFollower.speed = speed;
        }
    }

    private void AutoChargeTurbo()
    {
        if (isInTurbo || turboCooldownTimer > 0f) return;

        turboCharge += turboChargeRate * Time.deltaTime;
        turboCharge = Mathf.Clamp(turboCharge, 0, turboThreshold);
    }

    private void CheckTurboActivationInput()
    {
        if (isInTurbo || turboCooldownTimer > 0f || turboCharge < turboThreshold) return;

        bool activated = false;

        if (Input.GetKeyDown(KeyCode.W))
        {
            activated = true;
        }

        if (!activated && j != null && calibrated)
        {
            Quaternion currentRotation = j.GetVector();
            Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * currentRotation;

            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            float signedTwist = Vector3.Dot(axis, Vector3.forward) >= 0 ? angle : -angle;

            if (signedTwist > 180f) signedTwist -= 360f;
            if (signedTwist < -180f) signedTwist += 360f;

            bool isTwisting = Mathf.Abs(signedTwist) > 3f;

            if (isTwisting)
            {
                if (!isRumbling)
                {
                    j.SetRumble(150, 400, 0.2f);
                    isRumbling = true;
                }
                activated = true;
            }
            else if (isRumbling)
            {
                j.SetRumble(0, 0, 0);
                isRumbling = false;
            }
        }

        if (activated)
        {
            ActivateTurbo();
        }
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

        Debug.Log("TURBO ACTIVATED!");
    }

    private void HandleTurboState()
    {
        if (isInTurbo)
        {
            turboTimer -= Time.deltaTime;
            speed = turboSpeed;

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
        else
        {
            if (turboCooldownTimer > 0f)
            {
                turboCooldownTimer -= Time.deltaTime;
            }
        }
    }

    private void HandleTurningInput()
    {
        float input = 0f;

        if (j != null)
        {
            input = j.GetStick()[1]; // Horizontal Joy-Con stick input
        }

        if (Input.GetKey(KeyCode.D))
            input += 1f;
        if (Input.GetKey(KeyCode.A))
            input -= 1f;

        currentOffset += input * turnSpeed * Time.deltaTime;
        currentOffset = Mathf.Clamp(currentOffset, -maxHorizontalOffset, maxHorizontalOffset);

        if (splineFollower != null)
        {
            splineFollower.SetHorizontalOffset(currentOffset);
        }
    }

    private void UpdateUI()
    {
        if (speedText != null)
        {
            speedText.text = $"Speed: {speed * 10:F1} km/h";
        }
    }

    public float GetTurboChargeNormalized()
    {
        return turboCharge / turboThreshold;
    }
}
