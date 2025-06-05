using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;
using Unity.Mathematics;

public class JoyconRevController : MonoBehaviour
{
    private List<Joycon> joycons;
    private Joycon j;

    public float minAngle = 10f;
    public float maxAngle = 90f;
    public float maxSpeed = 30f;
    public float accelerationDecay = 3f;
    public float minSpeed = 0f;
    public float deadzone = 3f;
    public TextMeshProUGUI speedText;

    public float speed = 0f;
    public float wAcceleration = 10f;
    private bool isRumbling = false;

    private Quaternion initialRotation;
    private bool calibrated = false;

    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;

    private float laneSwitchCooldown = 0.3f;
    private float laneSwitchTimer = 0f;
    private bool stickInUse = false;
    public BikeSplineFollower splineFollower;

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
    }

    void Calibrate()
    {
        initialRotation = j.GetVector();
        calibrated = true;
        Debug.Log("Calibrated orientation.");
    }

    void Update()
    {
        HandleKeyAcceleration(); // Keyboard-based speed up

        if (j != null && calibrated)
        {
            HandleTwistSpeed();  // Gyro-based twist/turn
            
        }
        HandleLaneSwitch();

        UpdateUI();

        if (splineFollower != null)
        {
            splineFollower.speed = speed;
        }
    }

    private void HandleTwistSpeed()
    {
        Quaternion currentRotation = j.GetVector();
        Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * currentRotation;

        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        float signedTwist = Vector3.Dot(axis, Vector3.forward) >= 0 ? angle : -angle;

        if (signedTwist > 180f) signedTwist -= 360f;
        if (signedTwist < -180f) signedTwist += 360f;

        bool isTwisting = Mathf.Abs(signedTwist) > deadzone;

        if (isTwisting)
        {
            float clampedTwist = Mathf.Clamp(signedTwist, -maxAngle, maxAngle);

            if (clampedTwist < -minAngle)
            {
                float mappedSpeed = Mathf.InverseLerp(-minAngle, -maxAngle, clampedTwist) * maxSpeed;
                speed = Mathf.Lerp(speed, mappedSpeed, Time.deltaTime * 5f);

                if (!isRumbling)
                {
                    j.SetRumble(250, 600, 0.4f);
                    isRumbling = true;
                }
            }
            else if (clampedTwist > minAngle)
            {
                speed -= accelerationDecay * 2f * Time.deltaTime;
                if (speed < minSpeed) speed = minSpeed;

                if (isRumbling)
                {
                    j.SetRumble(0, 0, 0);
                    isRumbling = false;
                }
            }
        }
        else
        {
            speed -= accelerationDecay * Time.deltaTime;
            if (speed < minSpeed) speed = minSpeed;

            if (isRumbling)
            {
                j.SetRumble(0, 0, 0);
                isRumbling = false;
            }
        }

        Debug.Log($"Twist Angle: {signedTwist:F2} | Speed: {speed:F2}");
    }

    private void HandleLaneSwitch()
    {
        laneSwitchTimer -= Time.deltaTime;

        if (laneSwitchTimer <= 0f)
        {
            // --- Joy-Con Stick Input ---
            if (j != null)
            {
                float horizontal = j.GetStick()[1];
                if (horizontal > 0.5f)
                {
                    splineFollower.MoveLaneRight();
                    laneSwitchTimer = laneSwitchCooldown;
                    Debug.LogWarning($"Lane switched RIGHT (Joy-Con). Current lane: {splineFollower.currentLane}");
                }
                else if (horizontal < -0.5f)
                {
                    splineFollower.MoveLaneLeft();
                    laneSwitchTimer = laneSwitchCooldown;
                    Debug.LogWarning($"Lane switched LEFT (Joy-Con). Current lane: {splineFollower.currentLane}");
                }
            }

            // --- Keyboard Input ---
            if (Input.GetKeyDown(KeyCode.D))
            {
                splineFollower.MoveLaneRight();
                laneSwitchTimer = laneSwitchCooldown;
                Debug.LogWarning($"Lane switched RIGHT (Keyboard). Current lane: {splineFollower.currentLane}");
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                splineFollower.MoveLaneLeft();
                laneSwitchTimer = laneSwitchCooldown;
                Debug.LogWarning($"Lane switched LEFT (Keyboard). Current lane: {splineFollower.currentLane}");
            }
        }
    }




    private void UpdateUI()
    {
        if (speedText != null)
        {
            speedText.text = $"Speed: {speed * 10:F1} km/h";
        }
    }

    private void HandleKeyAcceleration()
    {
        if (Input.GetKey(KeyCode.W))
        {
            speed += wAcceleration * Time.deltaTime;
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
            Debug.Log("Accelerating with W key. Speed: " + speed);
        }
    }


}
