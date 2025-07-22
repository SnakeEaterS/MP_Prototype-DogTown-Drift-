using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.SceneManagement;

public class BikeSplineFollower : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 5f;

    private float t = 0f;

    // Physical horizontal offset (position on the road)
    private float currentHorizontalOffset = 0f;
    private float targetHorizontalOffset = 0f;

    [Header("Leaning Settings")]
    public float maxLeanAngle = 20f;
    public float leanSmoothSpeed = 5f;

    // Lean angle state
    private float currentLeanAngle = 0f;
    private float targetLeanAngle = 0f;

    private bool hasReachedEnd = false;

    [Header("Scene Transition")]
    public bool enableSceneTransition = true;
    public string nextSceneName;

    [Header("Loop Settings")]
    public bool loopSpline = false;

    public float GetSplineT() => t;

    // Called externally to set lateral offset (position)
    public void SetHorizontalOffset(float offset)
    {
        targetHorizontalOffset = offset;
    }

    // Called externally to set lean input normalized [-1..1]
    public void SetLeanInput(float normalizedInput)
    {
        normalizedInput = Mathf.Clamp(normalizedInput, -1f, 1f);
        targetLeanAngle = -normalizedInput * maxLeanAngle;
    }

    void Update()
    {
        if (hasReachedEnd && !loopSpline) return;

        // Smoothly interpolate lateral position offset
        currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, targetHorizontalOffset, leanSmoothSpeed * Time.deltaTime);

        // Smoothly interpolate lean angle
        currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, leanSmoothSpeed * Time.deltaTime);

        float splineLength = spline.CalculateLength();
        t += (speed * Time.deltaTime) / splineLength;

        if (loopSpline)
        {
            t %= 1f;
        }
        else
        {
            t = Mathf.Clamp01(t);
        }

        Vector3 position = spline.EvaluatePosition(t);
        Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        // Apply physical lateral offset
        transform.position = position + right * currentHorizontalOffset;

        // Base rotation aligned to spline tangent
        Quaternion baseRotation = Quaternion.LookRotation(tangent, Vector3.up);

        // Apply lean rotation around local Z axis
        transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, currentLeanAngle);

        // Scene transition logic
        if (t >= 1f && !hasReachedEnd && !loopSpline)
        {
            hasReachedEnd = true;

            if (!enableSceneTransition)
            {
                Debug.Log("Scene transition is disabled.");
                return;
            }

            if (string.IsNullOrEmpty(nextSceneName))
            {
                return;
            }

            Debug.Log("Reached end. Trying to load scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
