using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileHealth : MonoBehaviour
{
    public float health = 20f; // Health of the missile
    public BossAttacks bossAttacks; // Reference to the BossAttacks script

    void Start()
    {
        bossAttacks = GameObject.FindObjectOfType<BossAttacks>();
        if (bossAttacks == null)
        {
            Debug.LogError("[MissileHealth] BossAttacks script not found!");
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"[MissileHealth] Missile took {damage} damage, remaining health: {health}");
        if (health <= 0f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log("[MissileHealth] Missile exploded!");
        // Optional: Damage area
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(50f); // Example damage value
            }
        }
        if (bossAttacks != null)
        {
            bossAttacks.isSecondAttackRunning = false;
            bossAttacks.startSecondAttack = false; // <-- ADD THIS LINE
        }
        Destroy(gameObject); // Destroy missile after explosion
    }
}
