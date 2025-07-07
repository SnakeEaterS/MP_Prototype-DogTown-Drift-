using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;
    public Transform spawnPoint; // Optional, can be used to specify where the boss spawns
    // Start is called before the first frame update
    void Start()
    {
        // Instantiate the boss at spawnPoint's position/rotation
        GameObject boss = Instantiate(bossPrefab, spawnPoint.position, transform.rotation);
    }

}
