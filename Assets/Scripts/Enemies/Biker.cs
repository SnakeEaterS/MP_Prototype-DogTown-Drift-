using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biker : MonoBehaviour
{
    public float speed = 5f;
    public float lateralOffset = 3f; // Side distance from player
    private Transform player;
    private Vector3 targetOffset;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        // Determine whether to ride on left or right randomly
        float side = Random.value < 0.5f ? -1f : 1f;
        targetOffset = player.right * lateralOffset * side;
    }

    void Update()
    {
        Vector3 targetPosition = player.position + targetOffset;
        targetPosition.y = transform.position.y; // Keep Y fixed

        // Move towards target beside the player
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
