using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class BarrierSpawner : MonoBehaviour
{
    [Header("Barrier Prefabs")]
    public GameObject[] barrierPrefabs;

    public float spawnZOffset = 50f;
    public float lateralOffset = 2f;

    [Header("Speed-based Spawning")]
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2.5f;
    public float maxSpeed = 30f;

    public SplineContainer splineContainer;
    public BikeSplineFollower playerFollower;
    public JoyconRevController controller;

    [Header("Debug")]
    public bool debugLogs = false;

    void Start()
    {
        if (!ValidateInitialReferences())
        {
            enabled = false;
            return;
        }

        StartCoroutine(SpawnBarriers());
    }

    bool ValidateInitialReferences()
    {
        bool valid = true;

        if (splineContainer == null)
        {
            Debug.LogError("[BarrierSpawner] Missing splineContainer reference!");
            valid = false;
        }

        if (playerFollower == null)
        {
            playerFollower = FindObjectOfType<BikeSplineFollower>();
            if (playerFollower == null)
            {
                Debug.LogError("[BarrierSpawner] Missing playerFollower reference!");
                valid = false;
            }
        }

        if (controller == null)
        {
            controller = FindObjectOfType<JoyconRevController>();
            if (controller == null && debugLogs)
            {
                Debug.LogWarning("[BarrierSpawner] JoyconRevController not found. Defaulting speed to 0.");
            }
        }

        return valid;
    }

    IEnumerator SpawnBarriers()
    {
        while (true)
        {
            if (playerFollower == null || splineContainer == null)
            {
                if (debugLogs)
                    Debug.LogWarning("[BarrierSpawner] Skipping spawn due to missing references.");
                yield return new WaitForSeconds(1f);
                TryReacquireReferences();
                continue;
            }

            SpawnBarrier();

            float currentSpeed = controller != null ? controller.speed : 0f;
            float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);
            float dynamicInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, normalizedSpeed);

            yield return new WaitForSeconds(dynamicInterval);
        }
    }

    void SpawnBarrier()
    {
        if (barrierPrefabs == null || barrierPrefabs.Length == 0)
        {
            if (debugLogs)
                Debug.LogWarning("[BarrierSpawner] No barrier prefabs assigned!");
            return;
        }

        float splineLength = splineContainer.CalculateLength();
        if (splineLength <= 0f)
        {
            if (debugLogs)
                Debug.LogWarning("[BarrierSpawner] Invalid spline length.");
            return;
        }

        float playerT = playerFollower.GetSplineT();
        float offsetT = spawnZOffset / splineLength;
        float spawnT = Mathf.Clamp01(playerT + offsetT);

        Vector3 centerPos = splineContainer.EvaluatePosition(spawnT);
        Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(spawnT)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        int laneIndex = Random.Range(0, 3); // 0 = Left, 1 = Center, 2 = Right
        float laneOffset = (laneIndex - 1) * lateralOffset;
        Vector3 finalSpawnPos = centerPos + right * laneOffset;

        // Pick a valid prefab
        GameObject selectedPrefab = null;
        for (int i = 0; i < 10; i++) // attempt up to 10 times
        {
            selectedPrefab = barrierPrefabs[Random.Range(0, barrierPrefabs.Length)];
            if (selectedPrefab != null) break;
        }

        if (selectedPrefab == null)
        {
            if (debugLogs)
                Debug.LogWarning("[BarrierSpawner] All selected prefabs were null!");
            return;
        }

        Quaternion baseRotation = Quaternion.LookRotation(tangent);
        bool needsFlip = selectedPrefab.CompareTag("FlipY");
        Quaternion finalRotation = baseRotation * (needsFlip ? Quaternion.Euler(0, 90f, 0) : Quaternion.identity);

        Instantiate(selectedPrefab, finalSpawnPos, finalRotation);
    }

    void TryReacquireReferences()
    {
        if (playerFollower == null)
            playerFollower = FindObjectOfType<BikeSplineFollower>();

        if (controller == null)
            controller = FindObjectOfType<JoyconRevController>();

        if (debugLogs)
            Debug.Log("[BarrierSpawner] Attempted to reacquire references.");
    }
}
