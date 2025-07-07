using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.SceneManagement;

public class BikeSplineFollower : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 5f;

    private float t = 0f;
    private float currentHorizontalOffset = 0f;

    [Header("Leaning Settings")]
    public float maxLeanAngle = 20f;
    public float leanSmoothSpeed = 5f;

    private float currentLeanAngle = 0f;
    private bool hasReachedEnd = false;

    [Header("Scene Transition")]
    public bool enableSceneTransition = true;
    public string nextSceneName;

    [Header("Loop Settings")]
    public bool loopSpline = false;

    public float GetSplineT() => t;

    public void SetHorizontalOffset(float offset)
    {
        currentHorizontalOffset = offset;
    }

    void Update()
    {
        if (hasReachedEnd && !loopSpline) return;

        float splineLength = spline.CalculateLength();
        t += (speed * Time.deltaTime) / splineLength;

        if (loopSpline)
        {
            t %= 1f; // Wraps t back to 0 when it reaches 1
        }
        else
        {
            t = Mathf.Clamp01(t);
        }

        Vector3 position = spline.EvaluatePosition(t);
        Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        transform.position = position + right * currentHorizontalOffset;
        transform.forward = tangent;

        float targetLeanPercent = Mathf.Clamp(currentHorizontalOffset / (maxLeanAngle == 0 ? 1 : maxLeanAngle), -1f, 1f);
        float targetLeanAngle = -targetLeanPercent * maxLeanAngle;
        currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, leanSmoothSpeed * Time.deltaTime);
        transform.rotation *= Quaternion.AngleAxis(currentLeanAngle, Vector3.forward);

        // Trigger scene transition if we're at the end and looping is disabled
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
                Debug.LogError("nextSceneName is not set! Scene switch aborted.");
                return;
            }

            Debug.Log("Reached end. Trying to load scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
