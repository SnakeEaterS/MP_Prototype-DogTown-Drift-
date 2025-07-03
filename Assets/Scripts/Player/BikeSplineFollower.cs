using UnityEngine;
using UnityEngine.Splines;

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

    public float GetSplineT() => t;

    public void SetHorizontalOffset(float offset)
    {
        currentHorizontalOffset = offset;
    }

    void Update()
    {
        if (spline == null || spline.Spline.Count < 2) return;

        float splineLength = spline.CalculateLength();
        t += (speed * Time.deltaTime) / splineLength;
        t = Mathf.Clamp01(t);

        Vector3 position = spline.EvaluatePosition(t);
        Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        transform.position = position + right * currentHorizontalOffset;
        transform.forward = tangent;

        float targetLeanPercent = Mathf.Clamp(currentHorizontalOffset / (maxLeanAngle == 0 ? 1 : maxLeanAngle), -1f, 1f);
        float targetLeanAngle = -targetLeanPercent * maxLeanAngle;
        currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, leanSmoothSpeed * Time.deltaTime);
        transform.rotation *= Quaternion.AngleAxis(currentLeanAngle, Vector3.forward);
    }

    public float DistanceTravelledOnSpline()
    {
        return spline.CalculateLength() * t;
    }

}
