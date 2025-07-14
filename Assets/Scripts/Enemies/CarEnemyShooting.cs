using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEnemyShooting : MonoBehaviour
{
    public Transform player;          // Assign your player transform here
    public LineRenderer lineRenderer; // Assign the Line Renderer component here

    private bool isShooting = false;
    private Transform firePoint;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;  // Hide initially
        }
    }

    void Update()
    {
        if (isShooting)
        {
            // Enable and update line positions
            lineRenderer.enabled = true;

            // Start point is biker's firing point (e.g. this object's position)
            lineRenderer.SetPosition(0, firePoint.position);

            // End point is player's current position (so it follows)
            lineRenderer.SetPosition(1, player.position);
        }
        else
        {
            if (lineRenderer.enabled)
                lineRenderer.enabled = false;
        }
    }

    public void StartShootingBeam(Transform firePoint)
    {
        isShooting = true;
        this.firePoint = firePoint; // Store the fire point if needed
    }

    public void StopShootingBeam()
    {
        isShooting = false;
    }
}
