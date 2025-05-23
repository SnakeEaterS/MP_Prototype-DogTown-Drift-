
using UnityEngine;
using UnityEngine.Splines; // If using Unity Splines

public class SplineFollower : MonoBehaviour
{
    public SplineContainer spline;   // Assign your spline
    public float splineSpeed = 5f;
    public float distanceAlongSpline = 0f;
    public float lateralOffset = 0f; // Left (-), Center (0), Right (+)

    public float laneOffset = 2f; // Distance between lanes

    void Update()
    {
        distanceAlongSpline += splineSpeed * Time.deltaTime;

        float splineLength = spline.Spline.GetLength();
        if (distanceAlongSpline > splineLength)
            distanceAlongSpline = splineLength;

        Vector3 position = spline.Spline.EvaluatePosition(distanceAlongSpline);
        Vector3 tangent = spline.Spline.EvaluateTangent(distanceAlongSpline);
        Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;

        // Offset left/right for lanes
        position += normal * lateralOffset;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(tangent);
    }


    public void SetLane(int lane)
    {
        lateralOffset = (lane - 1) * laneOffset; // lane 0 = left, 1 = mid, 2 = right
    }
}
