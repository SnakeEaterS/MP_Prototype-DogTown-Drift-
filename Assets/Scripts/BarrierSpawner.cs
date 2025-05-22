using System.Collections;
using UnityEngine;

public class BarrierSpawner : MonoBehaviour
{
    public GameObject barrierPrefab;          // Prefab to spawn
    public Transform[] lanePositions;         // Lanes to spawn on (index 0 = Left, 1 = Middle, 2 = Right)
    public float spawnInterval = 2f;          // Time between spawns
    public float spawnZOffset = 50f;          // Distance in front of the player to spawn

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnBarriers());
    }

    IEnumerator SpawnBarriers()
    {
        while (true)
        {
            SpawnBarrier();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBarrier()
    {
        int laneIndex = Random.Range(0, lanePositions.Length);
        Vector3 spawnPosition = lanePositions[laneIndex].position;
        spawnPosition.z = player.position.z + spawnZOffset;

        Instantiate(barrierPrefab, spawnPosition, Quaternion.identity);
    }
}
