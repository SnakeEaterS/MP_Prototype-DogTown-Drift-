using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    private bool isRumbling = false;

    private Quaternion initialRotation;
    private bool calibrated = false;

    public Transform[] lanePositions; // Assign 3 lane Transforms in Inspector: Left, Middle, Right
    private int currentLane = 1;      // Start in the middle lane (index 1)
    public float laneSwitchSpeed = 5f;
    private float laneSwitchCooldown = 0.3f; // Minimum delay between lane changes
    private float laneSwitchTimer = 0f;

    private bool stickInUse = false; // Tracks if stick was pushed





    void Start()
    {
        joycons = JoyconManager.Instance.j;

        if (joycons.Count < 1)
        {
            Debug.LogWarning("No Joy-Cons connected");
            enabled = false;
            return;
        }

        j = joycons[0];
        Calibrate();
    }

    void Calibrate()
    {
        initialRotation = j.GetVector();
        calibrated = true;
        Debug.Log("Calibrated orientation.");
    }

    void Update()
    {
        if (j == null || !calibrated) return;

        Quaternion currentRotation = j.GetVector();
        Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * currentRotation;

        // Extract twist around Z axis only (Joy-Con forward axis)
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        float signedTwist = Vector3.Dot(axis, Vector3.forward) >= 0 ? angle : -angle;

        // Normalize angle to [-180, 180]
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

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        // Smoothly move to current lane
        Vector3 targetLanePos = new Vector3(lanePositions[currentLane].position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetLanePos, laneSwitchSpeed * Time.deltaTime);


        Debug.Log($"Twist Angle: {signedTwist:F2} | Speed: {speed:F2}");

        if (speedText != null)
        {
            speedText.text = $"Speed: {speed * 10:F1} km/h";
        }

        laneSwitchTimer -= Time.deltaTime;
        float horizontal = j.GetStick()[1];

        if (!stickInUse && laneSwitchTimer <= 0f)
        {
            if (horizontal > 0.5f && currentLane < 2)
            {
                currentLane++;
                stickInUse = true;
                laneSwitchTimer = laneSwitchCooldown;
            }
            else if (horizontal < -0.5f && currentLane > 0)
            {
                currentLane--;
                stickInUse = true;
                laneSwitchTimer = laneSwitchCooldown;
            }
        }

        if (Mathf.Abs(horizontal) < 0.2f)
        {
            stickInUse = false; // Reset when stick returns to center
        }


    }
}
