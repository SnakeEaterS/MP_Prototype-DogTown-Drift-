using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public float headshotMultiplier = 2f; // Damage multiplier for headshots

    void Start()
    {
        Destroy(gameObject, 3f); // Destroy after 3 seconds
    }

    private void OnTriggerEnter(Collider other)
    {
        float finalDamage = damage;

        if (other.CompareTag("EnemyHead"))
        {
            // Hit the enemy's head — deal bonus damage
            finalDamage *= headshotMultiplier;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(finalDamage);
            Destroy(gameObject);
        }
    }
}
