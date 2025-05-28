using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
public class EnemySpawner : MonoBehaviour
{
    public GameObject bikerPrefab;
    public GameObject dronePrefab;
    public Transform player;

    public float laneOffset = 4f; // Distance between paths
    public float spawnDistanceBehind = 10f;
    public float spawnInterval = 2f;
    public float minY = 0f;
    public float maxY = 3f;
    public float minXOffset = -6f;
    public float maxXOffset = 6f;

    public int maxEnemiesPerLane = 2;
    public int maxDrones = 6;

    private float timer = 0f;

    // Use path indices: -1 (left), 0 (center), 1 (right)
    private int[] laneIndices = new int[] { -1, 0, 1 };

    private List<GameObject> drones = new List<GameObject>();
    private Dictionary<int, List<GameObject>> bikers = new Dictionary<int, List<GameObject>>();

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
            //SpawnBikerEnemy();
            SpawnDroneEnemy();
            timer = 0f;
        }

        // Clean up destroyed enemies
        foreach (var key in bikers.Keys)
        {
            bikers[key].RemoveAll(e => e == null);
        }
    }

    void SpawnBikerEnemy()
    {
        // Get player.t using reflection
        float playerT = 0f;
        BikeSplineFollower playerFollower = player.GetComponent<BikeSplineFollower>();
        if (playerFollower != null)
        {
            var tField = typeof(BikeSplineFollower).GetField("t", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (tField != null)
            {
                playerT = (float)tField.GetValue(playerFollower);
            }
        }

        float enemyT = Mathf.Max(0f, playerT - 0.0075f);

        List<int> lanesToTry = new List<int> { -1, 1 };
        // Shuffle lanes
        for (int i = 0; i < lanesToTry.Count; i++)
        {
            int tmp = lanesToTry[i];
            int randIndex = Random.Range(i, lanesToTry.Count);
            lanesToTry[i] = lanesToTry[randIndex];
            lanesToTry[randIndex] = tmp;
        }

        foreach (int lane in lanesToTry)
        {
            // Skip if lane full
            if (bikers[lane].Count >= maxEnemiesPerLane)
                continue;

            foreach (GameObject enemy in bikers[lane])
            {
                if (enemy == null) continue;
                var follower = enemy.GetComponent<BikeSplineFollower>();
                if (follower != null)
                {
                    var tField = typeof(BikeSplineFollower).GetField("t", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    float existingT = (float)tField.GetValue(follower);
                }
            }
            // Spawn enemy on this lane
            Vector3 lateralOffset = player.right * lane * laneOffset;
            Vector3 backwardOffset = -player.forward * spawnDistanceBehind;
            Vector3 spawnPosition = player.position + lateralOffset + backwardOffset;
            spawnPosition.y = Random.Range(minY, maxY);

            GameObject enemyObj = Instantiate(bikerPrefab, spawnPosition, Quaternion.identity);
            Biker biker = enemyObj.GetComponent<Biker>();
            biker.spawnT = enemyT;
            bikers[lane].Add(enemyObj);

            break; // Spawned one enemy, stop trying lanes
        }
    }

    public void SpawnDroneEnemy()
    {
        // Clean up null references
        drones.RemoveAll(d => d == null);
        if (drones.Count >= maxDrones)
            return;

        // Random lateral position (not tied to lanes)
        float randomXOffset = Random.Range(minXOffset, maxXOffset);
        Vector3 lateralOffset = player.right * randomXOffset;

        // Random backward distance
        float randomBack = Random.Range(spawnDistanceBehind, spawnDistanceBehind + 10f);
        Vector3 backwardOffset = -player.forward * randomBack;

        // Random vertical Y within range
        float randomY = Random.Range(minY, maxY);

        // Final spawn position
        Vector3 spawnPosition = player.position + lateralOffset + backwardOffset;
        spawnPosition.y = randomY;

        GameObject droneObj = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);

        drones.Add(droneObj);
    }

}
