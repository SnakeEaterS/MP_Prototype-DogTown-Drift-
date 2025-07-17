using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    public float chargeSpeed = 50f;
    public float explosionRadius = 2f;
    public float damage = 50f;

    public BossAttacks bossAttacks; // Reference to the BossAttacks script

    private Transform player;
    private Vector3 chargeTarget;
    private float lifeTime;

    private bool isCharging = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        bossAttacks = GameObject.FindObjectOfType<BossAttacks>();
        if (player == null)
        {
            Debug.LogError("[Missile] Player not found!");
            return;
        }

        isCharging = true;
    }

    void Update()
    {
        lifeTime += Time.deltaTime;
        if (isCharging)
        {
            chargeTarget = player.position;
            transform.position = Vector3.MoveTowards(transform.position, player.position, chargeSpeed * Time.deltaTime);
            Vector3 direction = (player.position - transform.position).normalized;

            // Adjust for your model’s forward direction, for example if it faces X+:
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
            if (Vector3.Distance(transform.position, chargeTarget) < 0.5f)
            {
                Explode();
            }
        }

        if (lifeTime > 5f) // Lifetime check to prevent infinite missiles
        {
            Debug.LogWarning("[Missile] Missile lifetime exceeded, destroying.");
            Destroy(gameObject);
        }
    }

    void Explode()
    {
        Debug.Log("[Missile] Hit the player!");

        // Optional: Damage area
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        if (bossAttacks != null)
        {
            bossAttacks.isSecondAttackRunning = false;
            bossAttacks.startSecondAttack = false;

        }
        Destroy(gameObject); // Destroy missile after explosion
    }
}