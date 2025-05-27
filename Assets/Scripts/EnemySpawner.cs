using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public float laneOffset = 4f; // Distance between paths
    public float spawnDistanceBehind = 10f;
    public float spawnInterval = 2f;
    public float minY = 0f;
    public float maxY = 3f;
    public int maxEnemiesPerLane = 2;

    private float timer = 0f;

    // Use path indices: -1 (left), 0 (center), 1 (right)
    private int[] laneIndices = new int[] { -1, 0, 1 };
    private Dictionary<int, List<GameObject>> laneEnemies = new Dictionary<int, List<GameObject>>();

    void Start()
    {
        foreach (int index in laneIndices)
        {
            laneEnemies[index] = new List<GameObject>();
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

        // Clean up destroyed enemies
        foreach (var key in laneEnemies.Keys)
        {
            laneEnemies[key].RemoveAll(e => e == null);
        }
    }

    void SpawnEnemy()
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

        bool spawned = false;

        foreach (int lane in lanesToTry)
        {
            Debug.Log(lane);
            // Skip if lane full
            if (laneEnemies[lane].Count >= maxEnemiesPerLane)
                continue;

            foreach (GameObject enemy in laneEnemies[lane])
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

            GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            Biker biker = enemyObj.GetComponent<Biker>();
            biker.spawnT = enemyT;
            laneEnemies[lane].Add(enemyObj);

            spawned = true;
            break; // Spawned one enemy, stop trying lanes
        }

        // If not spawned, no lane was valid this frame
    }

}
