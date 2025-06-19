using UnityEngine;

[System.Serializable]
public class PhasePoint
{
    [Range(0f, 1f)] public float splineT;

    [Header("Spawning Control")]
    public bool enableEnemySpawning = true;
    public bool enableBarriers = true;
    public float enemySpawnRate = 2f;

    [Header("Enemy Types")]
    public bool allowBikers = true;
    public bool allowDrones = true;

    [Header("Drone Movement Overrides")]
    public float droneHoverAmplitude = 0.2f;
    public float droneHoverFrequency = 2f;
    public float droneDriftAmplitude = 1f;
    public float droneDriftFrequency = 1.5f;

    [HideInInspector] public bool foldout = true; // For foldout toggle
}
