using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEnemyShooting : MonoBehaviour
{
    public Transform player;          // Assign your player transform here
    public LineRenderer lineRenderer; // Assign the Line Renderer component here

    private bool isShooting = false;

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
            lineRenderer.SetPosition(0, transform.position);

            // End point is player's current position (so it follows)
            lineRenderer.SetPosition(1, player.position);
        }
        else
        {
            if (lineRenderer.enabled)
                lineRenderer.enabled = false;
        }
    }

    public void StartShootingBeam()
    {
        isShooting = true;
    }

    public void StopShootingBeam()
    {
        isShooting = false;
    }
}
