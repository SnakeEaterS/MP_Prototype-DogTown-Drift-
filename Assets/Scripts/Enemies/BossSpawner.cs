using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;
    public Transform spawnPoint; // Optional, can be used to specify where the boss spawns
    public GameObject warningUI;
    [Header("Spawn Delay")]
    public float spawnDelay = 3f; // Seconds to wait before spawning

    void Start()
    {
        // Start the delayed spawn
        StartCoroutine(SpawnBossWithDelay());
    }

    IEnumerator SpawnBossWithDelay()
    {
        warningUI.SetActive(true);
        yield return new WaitForSeconds(spawnDelay);
        warningUI.SetActive(false);
        if (bossPrefab != null)
        {
            Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("BossSpawner: No boss prefab assigned!");
        }
    }
}
