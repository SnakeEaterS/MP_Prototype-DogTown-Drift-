using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GamePhaseManager : MonoBehaviour
{
    public BikeSplineFollower playerFollower;
    public PhasePoint[] phases;

    private int currentPhaseIndex = -1;

    public EnemySpawner enemySpawner;
    public BarrierSpawner barrierSpawner;

    void Update()
    {
        if (playerFollower == null || phases == null || phases.Length == 0) return;

        float t = playerFollower.GetSplineT();

        int newPhaseIndex = currentPhaseIndex;

        for (int i = 0; i < phases.Length; i++)
        {
            if (t >= phases[i].splineT)
            {
                newPhaseIndex = i;
            }
        }

        if (newPhaseIndex != currentPhaseIndex)
        {
            currentPhaseIndex = newPhaseIndex;
            ApplyPhase(phases[currentPhaseIndex]);
        }
    }

    void ApplyPhase(PhasePoint phase)
    {
        Debug.Log($"Entered Phase {currentPhaseIndex} at t={phase.splineT}");

        if (enemySpawner != null)
        {
            enemySpawner.spawnInterval = phase.enemySpawnRate;
            enemySpawner.enabled = phase.enableEnemySpawning;
            enemySpawner.allowBikers = phase.allowBikers;
            enemySpawner.allowDrones = phase.allowDrones;
        }

        if (barrierSpawner != null)
        {
            barrierSpawner.enabled = phase.enableBarriers;
        }
    }

    void OnDrawGizmos()
    {
        if (playerFollower == null || phases == null || phases.Length == 0) return;

        var spline = playerFollower.spline;
        if (spline == null) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < phases.Length; i++)
        {
            float t = Mathf.Clamp01(phases[i].splineT);
            Vector3 pos = spline.EvaluatePosition(t);

            Gizmos.DrawSphere(pos, 0.5f);

#if UNITY_EDITOR
            Handles.Label(pos + Vector3.up * 1.5f, $"Phase {i}\nt={t:F2}");
#endif
        }
    }
}