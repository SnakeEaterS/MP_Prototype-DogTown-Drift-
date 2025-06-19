using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject bikerPrefab;
    public GameObject dronePrefab;
    public Transform player;

    public float laneOffset = 4f;
    public float spawnDistanceBehind = 10f;
    public float spawnInterval = 2f;
    public float minXOffset = -6f;
    public float maxXOffset = 6f;

    public int maxEnemiesPerLane = 2;
    public int maxDrones = 6;

    public bool allowBikers = true;
    public bool allowDrones = true;

    private float timer = 0f;

    private int[] laneIndices = new int[] { -1, 0, 1 };
    private List<GameObject> drones = new List<GameObject>();
    public Dictionary<int, List<GameObject>> bikers = new Dictionary<int, List<GameObject>>();

    private class LaneIndexTracker
    {
        public Queue<int> queue;
        public HashSet<int> set;

        public LaneIndexTracker(params int[] indices)
        {
            queue = new Queue<int>(indices);
            set = new HashSet<int>(indices);
        }
    }

    private Dictionary<int, LaneIndexTracker> indexTrackers = new Dictionary<int, LaneIndexTracker>();

    void Start()
    {
        InitDictionaries();
        TryFindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            TryFindPlayer();
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            bool bikerSpawned = allowBikers ? SpawnBikerEnemy() : false;
            bool droneSpawned = allowDrones ? SpawnDroneEnemy() : false;
            timer = 0f;

            if ((!bikerSpawned && allowBikers) || (!droneSpawned && allowDrones))
            {
                Debug.LogWarning("[EnemySpawner] Spawn skipped due to missing prefab or references.");
            }
        }

        // Clean up destroyed enemies
        foreach (var key in bikers.Keys.ToList())
        {
            bikers[key].RemoveAll(e => e == null);
        }

        drones.RemoveAll(d => d == null);
    }


    void InitDictionaries()
    {
        foreach (int index in laneIndices)
        {
            if (!bikers.ContainsKey(index))
                bikers[index] = new List<GameObject>();

            if ((index == -1 || index == 1) && !indexTrackers.ContainsKey(index))
                indexTrackers[index] = new LaneIndexTracker(0, 1);
        }
    }

    void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("[EnemySpawner] Player reference restored.");
        }
    }

    bool SpawnBikerEnemy()
    {
        if (bikerPrefab == null || player == null)
            return false;

        List<int> lanesToTry = new List<int> { -1, 1 };

        foreach (int lane in lanesToTry)
        {
            if (!bikers.ContainsKey(lane) || !indexTrackers.ContainsKey(lane))
                continue;

            if (bikers[lane].Count >= maxEnemiesPerLane || !indexTrackers[lane].queue.Any())
                continue;

            int bikerIndex = indexTrackers[lane].queue.Dequeue();
            indexTrackers[lane].set.Remove(bikerIndex);

            float laneSide = lane * laneOffset;
            float spacingZ = player.position.z - spawnDistanceBehind - (bikerIndex * 1f);
            Vector3 spawnPos = new Vector3(laneSide, 1f, spacingZ);

            GameObject enemyObj = Instantiate(bikerPrefab, spawnPos, Quaternion.identity);
            Biker biker = enemyObj.GetComponent<Biker>();

            if (biker != null)
            {
                biker.Initialize(this, lane, bikerIndex);
            }

            bikers[lane].Add(enemyObj);
            return true; // Successfully spawned
        }

        return false; // No spawn occurred
    }

    public bool SpawnDroneEnemy()
    {
        if (dronePrefab == null || player == null || drones.Count >= maxDrones)
            return false;

        float randomXOffset = Random.Range(minXOffset, maxXOffset);
        Vector3 lateralOffset = player.right * randomXOffset;
        Vector3 spawnPosition = player.position + lateralOffset;

        GameObject droneObj = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
        drones.Add(droneObj);
        return true;
    }

    public void ReturnBikerIndex(int lane, int index)
    {
        if (!indexTrackers.ContainsKey(lane)) return;

        var tracker = indexTrackers[lane];
        if (tracker.set.Add(index))
        {
            tracker.queue.Enqueue(index);
            Debug.Log($"[Spawner] Returned index {index} to lane {lane}");
        }
    }
}
