using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;

public class JoyconRevController : MonoBehaviour
{
    private List<Joycon> joycons;
    private Joycon j;

    public float baseSpeed = 15f;  // Base speed when not in turbo
    public float speed = 0f;
    private bool isRumbling = false;

    private Quaternion initialRotation;
    private bool calibrated = false;

    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;
    public BikeSplineFollower splineFollower;

    [Header("Turbo Settings")]
    public float turboDuration = 3f;
    public float turboChargeRate = 30f;   // Auto charge rate
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

        if (splineFollower != null)
        {
            // Sync lanes on start
            splineFollower.currentLane = splineFollower.targetLane = 1;
        }
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

        HandleLaneInput();

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
        if (isInTurbo || turboCooldownTimer > 0f) return;
        if (turboCharge < turboThreshold) return;

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

            bool isTwisting = Mathf.Abs(signedTwist) > 3f; // deadzone

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

    private void HandleLaneInput()
    {
        if (j != null)
        {
            float horizontal = j.GetStick()[1];

            if (horizontal > 0.5f)
            {
                splineFollower.MoveLaneRight();
            }
            else if (horizontal < -0.5f)
            {
                splineFollower.MoveLaneLeft();
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            splineFollower.MoveLaneRight();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            splineFollower.MoveLaneLeft();
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
