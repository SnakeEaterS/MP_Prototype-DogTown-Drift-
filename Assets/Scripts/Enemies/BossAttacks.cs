using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BossAttacks : MonoBehaviour
{
    public GameObject player;
    public GameObject shootingParticlePrefab;
    public GameObject[] firingPoints;
    public float damage = 10f;
    public float attackCooldown = 2f;
    public float attackPhase = 1;
    private float lastAttackTime = 0f;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    private void Start()
    {
        StartCoroutine(AttackPhases());
    }

    public IEnumerator AttackPhases()
    {
        while (true)
        {
            switch (attackPhase)
            {
                case 1:
                    PhaseOneAttack();
                    break;
                case 2:
                    PhaseTwoAttack();
                    break;
                case 3:
                    PhaseThreeAttack();
                    break;
                default:
                    Debug.LogWarning("Unknown attack phase.");
                    break;
            }

            // Delay to prevent constant looping
            yield return null;
        }
    }

    private void PhaseOneAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            DoBasicAttack();
            lastAttackTime = Time.time;
        }
    }

    private void PhaseTwoAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            DoBasicAttack();
            DoExtraAttack(); // add more logic here
            lastAttackTime = Time.time;
        }
    }

    private void PhaseThreeAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            DoHeavyAttack();
            lastAttackTime = Time.time;
        }
    }

    private void DoBasicAttack()
    {
        if (player != null && firingPoints.Length > 0 && shootingParticlePrefab != null)
        {
            // Select a random firing point
            int index = Random.Range(0, firingPoints.Length);
            GameObject selectedPoint = firingPoints[index];

            // Instantiate shooting particle at the firing point
            Instantiate(shootingParticlePrefab, selectedPoint.transform.position, selectedPoint.transform.rotation);

            // Damage the player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Basic Attack from point {index}!");
            }
        }
        else
        {
            Debug.LogWarning("Missing firing points, player, or shooting particle prefab.");
        }
    }

    private void DoExtraAttack()
    {
        Debug.Log("Extra Attack triggered in Phase 2!");
        // Add your effects, spawns, or mechanics
    }

    private void DoHeavyAttack()
    {
        Debug.Log("Heavy Attack triggered in Phase 3!");
        // Add heavy area damage, lasers, spikes, etc.
    }
}