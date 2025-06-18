using UnityEngine;
using UnityEngine.Splines;

public class BikeSplineFollower : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 5f;

    private float t = 0f;
    private float currentHorizontalOffset = 0f;

    [Header("Leaning Settings")]
    public float maxLeanAngle = 20f; // max degrees of lean
    public float leanSmoothSpeed = 5f; // how fast the lean interpolates

    private float currentLeanAngle = 0f; // current applied lean angle

    public float GetSplineT() => t;

    public void SetHorizontalOffset(float offset)
    {
        currentHorizontalOffset = offset;
    }

    void Update()
    {
        float splineLength = spline.CalculateLength();
        t += (speed * Time.deltaTime) / splineLength;
        t = Mathf.Clamp01(t);

        Vector3 position = spline.EvaluatePosition(t);
        Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        transform.position = position + right * currentHorizontalOffset;
        transform.forward = tangent;

        // Compute the target lean angle based on offset
        float targetLeanPercent = Mathf.Clamp(currentHorizontalOffset / (maxLeanAngle == 0 ? 1 : maxLeanAngle), -1f, 1f);
        float targetLeanAngle = -targetLeanPercent * maxLeanAngle; // lean into the turn

        // Smoothly interpolate current lean angle toward target lean angle
        currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, leanSmoothSpeed * Time.deltaTime);

        // Apply lean rotation around forward axis
        transform.rotation *= Quaternion.AngleAxis(currentLeanAngle, Vector3.forward);
    }
}
