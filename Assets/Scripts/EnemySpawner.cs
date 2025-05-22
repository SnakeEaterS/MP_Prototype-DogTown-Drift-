using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public Transform[] laneTransforms;
    public float spawnDistanceBehind = 10f;
    public float spawnInterval = 2f;
    public float minY = 0f;
    public float maxY = 3f;
    public int maxEnemiesPerLane = 2;

    private float[] laneXPositions;
    private float timer = 0f;
    private Dictionary<float, List<GameObject>> laneEnemies = new Dictionary<float, List<GameObject>>();

    void Start()
    {
        laneXPositions = new float[laneTransforms.Length];
        for (int i = 0; i < laneTransforms.Length; i++)
        {
            float laneX = laneTransforms[i].position.x;
            laneXPositions[i] = laneX;
            laneEnemies[laneX] = new List<GameObject>();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }

        // Clean up destroyed enemies from the list
        foreach (var lane in laneXPositions)
        {
            laneEnemies[lane].RemoveAll(e => e == null);
        }
    }

    void SpawnEnemy()
    {
        float playerX = Mathf.Round(player.position.x * 10) / 10f;

        List<float> availableLanes = new List<float>();
        foreach (float laneX in laneXPositions)
        {
            if (Mathf.Abs(laneX - playerX) > 0.01f && laneEnemies[laneX].Count < maxEnemiesPerLane)
            {
                availableLanes.Add(laneX);
            }
        }

        if (availableLanes.Count == 0)
            return; // No space to spawn

        float chosenX = availableLanes[Random.Range(0, availableLanes.Count)];

        Vector3 spawnDirection = -player.forward;
        Vector3 spawnPosition = player.position + spawnDirection * spawnDistanceBehind;

        spawnPosition.x = chosenX;
        spawnPosition.y = Random.Range(minY, maxY);

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        laneEnemies[chosenX].Add(enemy);
    }
}