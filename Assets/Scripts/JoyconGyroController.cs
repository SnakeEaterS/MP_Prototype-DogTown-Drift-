using System.Collections.Generic;
using UnityEngine;

public class JoyconRevController : MonoBehaviour
{
    private List<Joycon> joycons;
    private Joycon j;

    public float minAngle = 10f;         // Minimum twist angle to start moving (degrees)
    public float maxAngle = 90f;         // Max twist angle considered for speed scaling (degrees)
    public float maxSpeed = 20f;         // Max speed of bike
    public float accelerationDecay = 3f; // Speed decays by this per second when no twist
    public float minSpeed = 0f;          // Minimum speed (usually 0)

    public float speed = 0f;

    private bool isRumbling = false;



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
    }

    void Update()
    {
        if (j == null) return;

        Quaternion orientation = j.GetVector();
        Vector3 euler = orientation.eulerAngles;

        float currentAngle = euler.z > 180 ? euler.z - 360 : euler.z;
        float invertedAngle = -currentAngle;

        bool isRevving = invertedAngle > minAngle;

        if (isRevving)
        {
            float mappedSpeed = Mathf.InverseLerp(minAngle, maxAngle, invertedAngle) * maxSpeed;
            speed = Mathf.Lerp(speed, mappedSpeed, Time.deltaTime * 5f);

            // Start rumble if not already rumbling
            if (!isRumbling)
            {
                j.SetRumble(160, 320, 0.4f); // Constant rumble
                isRumbling = true;
            }
        }
        else
        {
            speed -= accelerationDecay * Time.deltaTime;
            if (speed < minSpeed) speed = minSpeed;

            // Stop rumble when not revving
            if (isRumbling)
            {
                j.SetRumble(0, 0, 0); // Stop rumble
                isRumbling = false;
            }
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        Debug.Log($"AngleZ: {currentAngle:F2} | Inverted: {invertedAngle:F2} | Speed: {speed:F2}");
    }




}
