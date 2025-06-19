using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhasePoint
{
    [Range(0f, 1f)] public float splineT;
    public bool enableEnemySpawning = true;
    public bool enableBarriers = true;
    public float enemySpawnRate = 2f;

    public bool allowBikers = true;
    public bool allowDrones = true;
}

