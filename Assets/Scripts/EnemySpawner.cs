using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public GameObject bikerPrefab;
    public GameObject dronePrefab;
    public Transform player;

    public float laneOffset = 4f; // Distance between paths
    public float spawnDistanceBehind = 10f;
    public float spawnInterval = 2f;
    public float minXOffset = -6f;
    public float maxXOffset = 6f;

    public int maxEnemiesPerLane = 2;
    public int maxDrones = 6;

    private float timer = 0f;

    private int[] laneIndices = new int[] { -1, 0, 1 };

    private List<GameObject> drones = new List<GameObject>();
    public Dictionary<int, List<GameObject>> bikers = new Dictionary<int, List<GameObject>>();

    // Class to manage both Queue and HashSet for index tracking
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
        foreach (int index in laneIndices)
        {
            bikers[index] = new List<GameObject>();

            // Only left and right lanes need index tracking
            if (index == -1 || index == 1)
            {
                indexTrackers[index] = new LaneIndexTracker(0, 1);
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnBikerEnemy();
            SpawnDroneEnemy();
            timer = 0f;
        }

        // Clean up destroyed bikers
        foreach (var key in bikers.Keys)
        {
            bikers[key].RemoveAll(e => e == null);
        }

        // Clean up destroyed drones
        drones.RemoveAll(d => d == null);
    }

    void SpawnBikerEnemy()
    {
        List<int> lanesToTry = new List<int> { -1, 1 };

        foreach (int lane in lanesToTry)
        {
            if (bikers[lane].Count >= maxEnemiesPerLane || !indexTrackers[lane].queue.Any())
                continue;

            int bikerIndex = indexTrackers[lane].queue.Dequeue();
            indexTrackers[lane].set.Remove(bikerIndex);

            float laneSide = lane * laneOffset;
            float spacingZ = player.position.z - spawnDistanceBehind - (bikerIndex * 1f); // adjusted spacing

            Vector3 spawnPos = new Vector3(laneSide, 1f, spacingZ);
            Quaternion spawnRot = Quaternion.identity;

            GameObject enemyObj = Instantiate(bikerPrefab, spawnPos, spawnRot);

            Biker biker = enemyObj.GetComponent<Biker>();
            biker.Initialize(this, lane, bikerIndex);

            bikers[lane].Add(enemyObj);
            break;
        }
    }


    public void SpawnDroneEnemy()
    {
        if (drones.Count >= maxDrones)
            return;

        float randomXOffset = Random.Range(minXOffset, maxXOffset);
        Vector3 lateralOffset = player.right * randomXOffset;
        Vector3 spawnPosition = player.position + lateralOffset;

        GameObject droneObj = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);

        drones.Add(droneObj);
    }

    public void ReturnBikerIndex(int lane, int index)
    {
        if (!indexTrackers.ContainsKey(lane)) return;

        var tracker = indexTrackers[lane];

        if (tracker.set.Add(index))  // Only add if not already in set
        {
            tracker.queue.Enqueue(index);
            Debug.Log($"[Spawner] Returned index {index} to lane {lane}");
        }
    }
}