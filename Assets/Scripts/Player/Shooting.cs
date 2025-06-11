using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public Transform firePoint;
    public float headshotMultiplier = 2f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }
    void Shoot()
    {
        // Visualize the raycast in Scene view
        Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            float finalDamage = damage;

            // Check if we hit the head
            if (hit.collider.CompareTag("EnemyHead"))
            {
                finalDamage *= headshotMultiplier;
            }

            // Try to get the Enemy script from parent (for head colliders)
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
            }

            // Optional: impact effects
            // Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}
