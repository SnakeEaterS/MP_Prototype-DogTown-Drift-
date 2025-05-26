using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class BarrierSpawner : MonoBehaviour
{
    public GameObject barrierPrefab;
    public float spawnZOffset = 50f;  // Distance forward on the spline
    public float lateralOffset = 2f;

    [Header("Speed-based Spawning")]
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2.5f;
    public float maxSpeed = 30f;

    public SplineContainer splineContainer;
    public BikeSplineFollower playerFollower;
    public JoyconRevController controller; // to get player speed

    void Start()
    {
        if (playerFollower == null || splineContainer == null)
        {
            Debug.LogError("Missing references for BarrierSpawner!");
            enabled = false;
            return;
        }

        if (controller == null)
        {
            controller = FindObjectOfType<JoyconRevController>();
        }

        StartCoroutine(SpawnBarriers());
    }

    IEnumerator SpawnBarriers()
    {
        while (true)
        {
            SpawnBarrier();

            float currentSpeed = controller != null ? controller.speed : 0f;
            float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);
            float dynamicInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, normalizedSpeed);

            yield return new WaitForSeconds(dynamicInterval);
        }
    }

    void SpawnBarrier()
    {
        float playerT = playerFollower.GetSplineT();
        float splineLength = splineContainer.CalculateLength();

        // Convert offset in world units to offset in normalized spline space (t)
        float offsetT = spawnZOffset / splineLength;
        float spawnT = Mathf.Clamp01(playerT + offsetT);

        Vector3 centerPos = (Vector3)splineContainer.EvaluatePosition(spawnT);
        Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(spawnT)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        int laneIndex = Random.Range(0, 3); // 0 = Left, 1 = Middle, 2 = Right
        float laneOffset = (laneIndex - 1) * lateralOffset;

        Vector3 finalSpawnPos = centerPos + right * laneOffset;

        Instantiate(barrierPrefab, finalSpawnPos, Quaternion.LookRotation(tangent));
    }
}
