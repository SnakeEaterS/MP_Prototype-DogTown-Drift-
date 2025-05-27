using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Biker : MonoBehaviour
{
    public float speed = 5f;
    public float pathOffset = 4f; // Distance between each path
    private Transform player;
    private Vector3 spawnDirection;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        // Choose left (-1) or right (+1) path relative to player
        int laneDirection = Random.value < 0.5f ? -1 : 1;

        // Calculate the spawn position using player's position and right vector
        Vector3 offset = player.right * pathOffset * laneDirection;
        transform.position = player.position + offset;

        // Set forward direction to match player's forward (aligned along the path)
        spawnDirection = player.forward;
        spawnDirection.y = 0f; 
        spawnDirection.Normalize();
    }

    void Update()
    {
        transform.position += spawnDirection * speed * Time.deltaTime;
    }
}   