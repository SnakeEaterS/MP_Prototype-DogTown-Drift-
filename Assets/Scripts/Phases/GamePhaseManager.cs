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

    public static float CurrentDroneHoverAmplitude { get; private set; } = 0.2f;
    public static float CurrentDroneHoverFrequency { get; private set; } = 2f;
    public static float CurrentDroneDriftAmplitude { get; private set; } = 1f;
    public static float CurrentDroneDriftFrequency { get; private set; } = 1.5f;

    void Start()
    {
        if (barrierSpawner != null && phases != null && phases.Length > 0)
        {
            for (int i = 0; i < phases.Length; i++)
            {
                if (phases[i].enableBarriers)
                {
                    float startT = phases[i].splineT;
                    float endT = (i + 1 < phases.Length) ? phases[i + 1].splineT : 1f;

                    float spread = phases[i].barrierSpread;
                    BarrierPattern pattern = phases[i].barrierPattern;

                    barrierSpawner.SpawnBarriersBetweenPhases(startT, endT, spread, pattern);
                }
            }
        }
    }

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

        CurrentDroneHoverAmplitude = phase.droneHoverAmplitude;
        CurrentDroneHoverFrequency = phase.droneHoverFrequency;
        CurrentDroneDriftAmplitude = phase.droneDriftAmplitude;
        CurrentDroneDriftFrequency = phase.droneDriftFrequency;
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
