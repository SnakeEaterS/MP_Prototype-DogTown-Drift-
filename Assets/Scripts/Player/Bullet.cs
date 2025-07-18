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
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
                Destroy(gameObject);
            }
            else 
            {
                MissileHealth missile = other.GetComponent<MissileHealth>();
                if (missile != null)
                {
                    missile.TakeDamage(finalDamage);
                    Destroy(gameObject);
                }
            }
        }
        else if (other.CompareTag("LeftWing") || other.CompareTag("RightWing"))
        {
            // Handle wing hit
            BossHealth boss = other.GetComponentInParent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(finalDamage);
                Destroy(gameObject);
            }
        }
    }
}
