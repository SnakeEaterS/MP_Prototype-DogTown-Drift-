using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEnemyShooting : MonoBehaviour
{
    public LineRenderer lineRenderer; // Assign the Line Renderer component here
    public float beamLength = 100f;   // How far the beam should shoot

    private bool isShooting = false;
    private Transform firePoint;

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;  // Hide initially
        }
    }

    void Update()
    {
        if (isShooting && firePoint != null)
        {
            lineRenderer.enabled = true;

            // Start from firePoint
            lineRenderer.SetPosition(0, firePoint.position);

            // Shoot forward from firePoint's forward direction
            lineRenderer.SetPosition(1, firePoint.position + firePoint.forward * beamLength);
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
        this.firePoint = firePoint;
    }

    public void StopShootingBeam()
    {
        isShooting = false;
    }
}
