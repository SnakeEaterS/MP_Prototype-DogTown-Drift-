using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public Transform firePoint;
    public RectTransform crosshairUI;
    public Canvas canvas;
    public LayerMask shootableLayer;
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
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position);
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);

        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range, shootableLayer))
        {
            float finalDamage = damage;

            if (hit.collider.CompareTag("EnemyHead"))
            {
                finalDamage *= headshotMultiplier;
            }

            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
            }
        }
    }
}
