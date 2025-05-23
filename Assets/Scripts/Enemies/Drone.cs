using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Drone : MonoBehaviour
{
    public float floatSpeed = 5f;
    public float hoverDistance = 10f;
    public float shootInterval = 2f;
    public GameObject bulletPrefab;
    public Transform shootPoint;

    public float xRange = 3f; // How far left/right they can hover
    public float yRange = 2f; // How high/low they can hover

    private Transform player;
    private float shootTimer;

    private Vector3 localOffset; // Fixed offset relative to player

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        // Generate random offset within given range
        float xOffset = Random.Range(-xRange, xRange);
        float yOffset = Random.Range(0, yRange);
        localOffset = new Vector3(xOffset, yOffset, 0f);
    }

    void Update()
    {
        // Position in front of player with offset
        Vector3 targetPosition = player.position + player.forward * hoverDistance;
        targetPosition += player.right * localOffset.x;
        targetPosition.y += localOffset.y;

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * floatSpeed * Time.deltaTime;

        // Shooting logic
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab && shootPoint)
        {
            Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        }
    }
}
