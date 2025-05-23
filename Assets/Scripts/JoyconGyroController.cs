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
    private bool isRumbling = false;

    private Quaternion initialRotation;
    private bool calibrated = false;

    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;

    private float laneSwitchCooldown = 0.3f;
    private float laneSwitchTimer = 0f;
    private bool stickInUse = false;
    public BikeSplineFollower splineFollower;

    public LineRenderer lineRenderer;   // Reference to the LineRenderer
    public int lineResolution = 50;     // How smooth the line will be


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

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }

        lineRenderer.positionCount = lineResolution + 1;
        lineRenderer.widthMultiplier = 0.1f;

        // Set a simple material if none assigned
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
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

        HandleTwistSpeed();
        HandleLaneSwitch();
        UpdateUI();

        if (splineFollower != null)
        {
            splineFollower.speed = speed;
        }
        UpdateLineRenderer();

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
        float horizontal = j.GetStick()[1]; // 1 = X axis on Joy-Con stick

        if (!stickInUse && laneSwitchTimer <= 0f)
        {
            if (horizontal > 0.5f)
            {
                splineFollower.MoveLaneRight();
                stickInUse = true;
                laneSwitchTimer = laneSwitchCooldown;
                Debug.LogWarning($"Lane switched RIGHT. Current lane: {splineFollower.currentLane}");
            }
            else if (horizontal < -0.5f)
            {
                splineFollower.MoveLaneLeft();
                stickInUse = true;
                laneSwitchTimer = laneSwitchCooldown;
                Debug.LogWarning($"Lane switched LEFT. Current lane: {splineFollower.currentLane}");
            }
        }

        if (Mathf.Abs(horizontal) < 0.2f)
        {
            stickInUse = false;
        }
    }


    private void UpdateUI()
    {
        if (speedText != null)
        {
            speedText.text = $"Speed: {speed * 10:F1} km/h";
        }
    }

    void UpdateLineRenderer()
    {
        if (splineContainer == null || lineRenderer == null) return;

        for (int i = 0; i <= lineResolution; i++)
        {
            float t = i / (float)lineResolution;
            Vector3 pos = (Vector3)splineContainer.EvaluatePosition(t);
            lineRenderer.SetPosition(i, pos);
        }
    }

}
