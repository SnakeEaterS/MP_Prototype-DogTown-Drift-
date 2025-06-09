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

    // Use path indices: -1 (left), 0 (center), 1 (right)
    private int[] laneIndices = new int[] { -1, 0, 1 };

    private List<GameObject> drones = new List<GameObject>();
    public Dictionary<int, List<GameObject>> bikers = new Dictionary<int, List<GameObject>>();

    void Start()
    {
        foreach (int index in laneIndices)
        {
            bikers[index] = new List<GameObject>();
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

        // Clean up destroyed enemies from bikers lists
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

        for (int i = 0; i < lanesToTry.Count; i++)
        {
            int lane = lanesToTry[i];

            if (bikers[lane].Count >= maxEnemiesPerLane)
                continue;

            GameObject enemyObj = Instantiate(bikerPrefab);
            enemyObj.transform.SetParent(player);

            float laneSide = lane * laneOffset;

            // Calculate spawn position offset based on how many bikers are already in the lane
            int bikerIndex = bikers[lane].Count;
            float spacingZ = -spawnDistanceBehind - (bikerIndex * 2f); // e.g. 2 units apart behind player

            enemyObj.transform.localPosition = new Vector3(laneSide, 1f, spacingZ);
            enemyObj.transform.localRotation = Quaternion.identity;

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
}
