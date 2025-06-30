using UnityEngine;
using UnityEngine.Splines;

public class BarrierSpawner : MonoBehaviour
{
    [Header("Barrier Prefabs")]
    public GameObject[] barrierPrefabs;

    public float lateralOffset = 2f;
    public float spacing = 10f; // Distance between barrier rows

    public SplineContainer splineContainer;

    [Header("Debug")]
    public bool debugLogs = false;

    void Start()
    {
        if (!ValidateInitialReferences())
        {
            enabled = false;
        }
    }

    bool ValidateInitialReferences()
    {
        if (splineContainer == null)
        {
            Debug.LogError("[BarrierSpawner] Missing splineContainer reference!");
            return false;
        }
        return true;
    }

    public void SpawnBarriersBetweenPhases(float startT, float endT, float spreadMultiplier = 1f, BarrierPattern pattern = BarrierPattern.SingleRandomLane)
    {
        if (barrierPrefabs == null || barrierPrefabs.Length == 0 || splineContainer == null)
            return;

        float splineLength = splineContainer.CalculateLength();
        float startDistance = startT * splineLength;
        float endDistance = endT * splineLength;

        int zigzagCounter = 0;

        for (float d = startDistance; d <= endDistance; d += spacing)
        {
            float t = d / splineLength;
            Vector3 centerPos = splineContainer.EvaluatePosition(t);
            Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(t)).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            float spread = lateralOffset * spreadMultiplier;
            bool[] lanes = GetLanesForPattern(pattern, zigzagCounter);
            zigzagCounter++;

            for (int laneIndex = 0; laneIndex < lanes.Length; laneIndex++)
            {
                if (!lanes[laneIndex]) continue;

                float laneOffset = (laneIndex - 1) * spread;
                Vector3 finalSpawnPos = centerPos + right * laneOffset;

                GameObject selectedPrefab = null;
                for (int i = 0; i < 10; i++)
                {
                    selectedPrefab = barrierPrefabs[Random.Range(0, barrierPrefabs.Length)];
                    if (selectedPrefab != null) break;
                }

                if (selectedPrefab == null)
                {
                    if (debugLogs)
                        Debug.LogWarning("[BarrierSpawner] All selected prefabs were null!");
                    continue;
                }

                Quaternion baseRotation = Quaternion.LookRotation(tangent);
                bool needsFlip = selectedPrefab.CompareTag("FlipY");
                Quaternion finalRotation = baseRotation * (needsFlip ? Quaternion.Euler(0, 90f, 0) : Quaternion.identity);

                Instantiate(selectedPrefab, finalSpawnPos, finalRotation);
            }
        }
    }

    bool[] GetLanesForPattern(BarrierPattern pattern, int zigzagCounter)
    {
        // Lane index: 0 = Left, 1 = Center, 2 = Right
        bool[] lanes = new bool[3];

        switch (pattern)
        {
            case BarrierPattern.SingleRandomLane:
                lanes[Random.Range(0, 3)] = true;
                break;

            case BarrierPattern.GapLeft:
                lanes[1] = lanes[2] = true;
                break;

            case BarrierPattern.GapRight:
                lanes[0] = lanes[1] = true;
                break;

            case BarrierPattern.ZigZag:
                lanes[zigzagCounter % 3] = true;
                break;

            case BarrierPattern.RandomTwoLanes:
                int first = Random.Range(0, 3);
                int second;
                do
                {
                    second = Random.Range(0, 3);
                } while (second == first);
                lanes[first] = true;
                lanes[second] = true;
                break;
        }

        return lanes;
    }
}
