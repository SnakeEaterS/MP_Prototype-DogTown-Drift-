using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCarsInEnviroment : MonoBehaviour
{
    public Transform[] spawnPoints; // Array of spawn points for cars
    public GameObject carPrefab; // Prefab of the car to spawn
    public GameObject truckPrefab; // Prefab of the truck to spawn
    public float spawnInterval = 3f; // Time between spawns
    public int maxSpawnedVehicles = 10; // Maximum number of vehicles in the scene
    public float speed = 100f; // Speed of the vehicles

    private float timer = 0f;
    private int currentSpawnedVehicles = 0;

    void Start()
    {
        // Initial spawn
        SpawnRandomVehicle();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentSpawnedVehicles < maxSpawnedVehicles)
        {
            SpawnRandomVehicle();
            timer = 0f;
        }
    }

    void SpawnRandomVehicle()
    {
        if (spawnPoints.Length == 0 || (carPrefab == null && truckPrefab == null))
        {
            Debug.LogWarning("Spawn points or prefabs not set up!");
            return;
        }

        // Choose a random spawn point
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];

        // Decide which vehicle to spawn (50% chance for each)
        GameObject vehicleToSpawn = Random.Range(0, 2) == 0 ? carPrefab : truckPrefab;

        // If one prefab is null, use the other one
        if (carPrefab == null) vehicleToSpawn = truckPrefab;
        if (truckPrefab == null) vehicleToSpawn = carPrefab;

        // Spawn the vehicle
        if (vehicleToSpawn != null)
        {
            GameObject car = Instantiate(vehicleToSpawn, spawnPoint.position, spawnPoint.rotation);
            car.GetComponentInChildren<LoopingMover>().minSpeed = speed; // Set the speed of the vehicle
            car.GetComponentInChildren<LoopingMover>().maxSpeed = speed; // Set the speed of the vehicle
            currentSpawnedVehicles++;
        }
    }


}
