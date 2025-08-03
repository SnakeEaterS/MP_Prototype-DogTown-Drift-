using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    public float chargeSpeed = 50f;
    public float explosionRadius = 2f;
    public float damage = 50f;
    public GameObject explosionPrefab;
    public BossAttacks bossAttacks;

    private Transform player;
    private float lifeTime;
    private float spawnTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        bossAttacks = GameObject.FindObjectOfType<BossAttacks>();

        if (player == null)
        {
            Debug.LogError("[Missile] Player not found!");
            return;
        }

        // Attach to player
        transform.SetParent(player);
        spawnTime = Time.time;
    }

    void Update()
    {
        lifeTime += Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, player.position, chargeSpeed * Time.deltaTime);
        Vector3 direction = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);

        if (Vector3.Distance(transform.position, player.position) < 0.5f)
        {
            Explode();
        }
        
        if (lifeTime > 5f)
        {
            CleanupAndDestroy();
        }
    }

    void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= 2f;

            // Add follow behavior without parenting
            var followScript = explosion.AddComponent<FollowTargetTemporary>();
            followScript.target = player.transform; // Enemy
            followScript.duration = 1f; // Match explosion VFX length
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        CleanupAndDestroy();
    }

    void CleanupAndDestroy()
    {
        if (bossAttacks != null)
        {
            bossAttacks.isSecondAttackRunning = false;
            bossAttacks.startSecondAttack = false;
        }
        Destroy(gameObject);
    }
}