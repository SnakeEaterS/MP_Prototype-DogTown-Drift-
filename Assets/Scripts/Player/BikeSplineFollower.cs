using UnityEngine;
using UnityEngine.Splines;

public class BikeSplineFollower : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 5f;

    private float t = 0f;
    private float currentHorizontalOffset = 0f;

    public float GetSplineT() => t;

    // Called by JoyconRevController to update horizontal offset
    public void SetHorizontalOffset(float offset)
    {
        currentHorizontalOffset = offset;
    }

    void Update()
    {
        // Move forward along the spline
        float splineLength = spline.CalculateLength();
        t += (speed * Time.deltaTime) / splineLength;
        t = Mathf.Clamp01(t);

        // Get position and direction along the spline
        Vector3 position = (Vector3)spline.EvaluatePosition(t);
        Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;


        // Apply horizontal (lateral) offset
        transform.position = position + right * currentHorizontalOffset;
        transform.forward = tangent;
    }
}
