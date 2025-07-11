using UnityEngine;

public enum BarrierPattern
{
    SingleRandomLane,
    GapLeft,
    GapRight,
    ZigZag,
    RandomTwoLanes
}

[System.Serializable]
public class PhasePoint
{
    [Range(0f, 1f)] public float splineT;

    [Header("Spawning Control")]
    public bool enableEnemySpawning = true;
    public bool enableBarriers = true;
    public float enemySpawnRate = 2f;

    [Header("Enemy Types")]
    public bool allowCars = true;
    public bool allowDrones = true;

    [Header("Drone Movement Overrides")]
    public float droneHoverAmplitude = 0.2f;
    public float droneHoverFrequency = 2f;
    public float droneDriftAmplitude = 1f;
    public float droneDriftFrequency = 1.5f;

    [Header("Barrier Settings")]
    [Range(0f, 2f)] public float barrierSpread = 1f;
    public BarrierPattern barrierPattern = BarrierPattern.SingleRandomLane;

    [HideInInspector] public bool foldout = true;
}
