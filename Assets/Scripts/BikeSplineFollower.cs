using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class BikeSplineFollower : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 5f;
    public float lateralOffset = 2f;

    public int currentLane = 1; // current lane index (0 = left, 1 = middle, 2 = right)
    public int targetLane = 1; // the lane we want to move towards

    private float t = 0f;

    public int maxLane = 2;
    public int minLane = 0;

    public float laneSwitchSpeed = 5f; // how fast we interpolate to the target lane

    private float currentLateralOffset = 0f; // current lateral offset value for smooth interpolation

    public float GetSplineT() => t;


    public void MoveLaneLeft()
    {
        if (targetLane > minLane)
            targetLane--;
    }

    public void MoveLaneRight()
    {
        if (targetLane < maxLane)
            targetLane++;
    }

    void Update()
    {
        float splineLength = spline.CalculateLength();
        t += (speed * Time.deltaTime) / splineLength;
        t = Mathf.Clamp01(t);

        // Calculate the target lateral offset based on targetLane
        float targetOffset = (targetLane - 1) * lateralOffset;

        // Smoothly interpolate current lateral offset towards target offset
        currentLateralOffset = Mathf.Lerp(currentLateralOffset, targetOffset, Time.deltaTime * laneSwitchSpeed);

        Vector3 position = (Vector3)spline.EvaluatePosition(t);
        Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        // Apply smooth lateral offset
        transform.position = position + right * currentLateralOffset;
        transform.forward = tangent;

        // Update currentLane only when interpolation is close to target
        if (Mathf.Abs(currentLateralOffset - targetOffset) < 0.01f)
        {
            currentLane = targetLane;
        }
    }
}
