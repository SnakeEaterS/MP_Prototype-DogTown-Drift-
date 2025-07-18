    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public Transform firePoint;
    public Transform gunFire;

    public GameObject explosionPrefab;
    public RectTransform crosshairUI;
    public Canvas canvas;
    
    public float damage = 10f;
    public float range = 100f;

    public LayerMask shootableLayer;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }
    void Shoot()
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position);
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);

        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 1f);

        RaycastHit hit;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, gunFire.position, gunFire.rotation);

            // Add follow behavior without parenting
            var followScript = explosion.AddComponent<FollowTargetTemporary>();
            followScript.target = gunFire.transform; // Enemy
            followScript.duration = 2f; // Match explosion VFX length
        }


        if (Physics.Raycast(ray, out hit, range, shootableLayer))
        {
            float finalDamage = damage;

            if (hit.collider.CompareTag("Enemy"))
            {
                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(finalDamage);
                }
            }
            else if (hit.collider.CompareTag("LeftWing") || hit.collider.CompareTag("RightWing"))
            {
                // Handle wing hit
                BossHealth boss = hit.collider.GetComponent<BossHealth>();
                if (boss != null)
                {
                    boss.TakeDamage(finalDamage);
                }
            }
            else if (hit.collider.CompareTag("Missile"))
            {
                MissileHealth missile = hit.collider.GetComponent<MissileHealth>();
                if (missile != null)
                {
                    missile.TakeDamage(finalDamage);
                }
            }
            else
            {
                Debug.Log($"[Shooting] Hit {hit.collider.name} but no damage applied.");
            }
        }
    }
}
