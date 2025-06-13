using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class BarrierSpawner : MonoBehaviour
{
    [Header("Barrier Prefabs")]
    public GameObject[] barrierPrefabs; // Multiple prefabs

    public float spawnZOffset = 50f;  // Distance forward on the spline
    public float lateralOffset = 2f;

    [Header("Speed-based Spawning")]
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2.5f;
    public float maxSpeed = 30f;

    public SplineContainer splineContainer;
    public BikeSplineFollower playerFollower;
    public JoyconRevController controller;

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
        if (barrierPrefabs.Length == 0)
        {
            Debug.LogWarning("No barrier prefabs assigned!");
            return;
        }

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

        // Randomly pick a prefab
        GameObject selectedPrefab = barrierPrefabs[Random.Range(0, barrierPrefabs.Length)];

        // Get base rotation along spline
        Quaternion baseRotation = Quaternion.LookRotation(tangent);

        // Check if the prefab needs a Y-axis flip
        bool needsFlip = selectedPrefab.CompareTag("FlipY"); // optional: add tag "FlipY" to those prefabs
        Quaternion finalRotation = baseRotation * (needsFlip ? Quaternion.Euler(0, 90f, 0) : Quaternion.identity);

        // Spawn it
        Instantiate(selectedPrefab, finalSpawnPos, finalRotation);
    }
}
