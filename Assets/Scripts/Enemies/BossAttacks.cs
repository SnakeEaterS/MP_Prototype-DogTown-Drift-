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

    [Header("Warning Indicator Settings")]
    public GameObject warningIndicatorPrefab; // assign in Inspector
    public float warningDuration = 1.5f;      // how long player has to dodge
    public float warningYOffset = -1f;        // Y offset for the warning indicator position
    [Range(0f, 1f)]
    public float followSmoothness = 0.05f;    // How quickly the warning follows the player (lower = more delay)

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
            DoExtraAttack();
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
        if (player != null && firingPoints.Length > 0 && warningIndicatorPrefab != null)
        {
            // Select a random firing point
            int index = Random.Range(0, firingPoints.Length);
            GameObject selectedPoint = firingPoints[index];

            // Get player's current position
            Vector3 predictedPos = player.transform.position;

            // Position warning with Y offset
            Vector3 indicatorPos = new Vector3(predictedPos.x, predictedPos.y + warningYOffset, predictedPos.z);

            // No rotation needed for cylinder ? just use identity
            Quaternion warningRotation = Quaternion.identity;

            // Spawn warning indicator
            GameObject warning = Instantiate(warningIndicatorPrefab, indicatorPos, warningRotation);

            Debug.Log($"[Boss] Warning placed at X:{indicatorPos.x}, Y:{indicatorPos.y}, Z:{indicatorPos.z}");

            // Start coroutine to fire after warning, with warning tracking player with delay
            StartCoroutine(FirePreciseShot(selectedPoint.transform.position, warning));

            // Optional: Play muzzle flash or shoot particle immediately
            Instantiate(shootingParticlePrefab, selectedPoint.transform.position, selectedPoint.transform.rotation);
        }
        else
        {
            Debug.LogWarning("Missing firing points, player, or warning indicator prefab.");
        }
    }

    private IEnumerator FirePreciseShot(Vector3 fireOrigin, GameObject warning)
    {
        float elapsed = 0f;
        Vector3 trackedPos = warning.transform.position;

        while (elapsed < warningDuration)
        {
            if (player != null && warning != null)
            {
                Vector3 playerPos = player.transform.position;

                // Desired target position (XZ follows player, Y fixed with offset)
                Vector3 desiredPos = new Vector3(playerPos.x, warning.transform.position.y, playerPos.z);

                // Smooth follow ? slowly move towards desired position
                warning.transform.position = Vector3.Lerp(warning.transform.position, desiredPos, followSmoothness);

                trackedPos = warning.transform.position;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (warning != null) Destroy(warning);

        Vector3 fireDir = (trackedPos - fireOrigin).normalized;

        RaycastHit hit;
        if (Physics.Raycast(fireOrigin, fireDir, out hit, 100f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log("[Boss] Player HIT by precise shot!");
                }
            }
            else
            {
                Debug.Log("[Boss] Shot missed player, hit: " + hit.collider.name);
            }
        }
    }

    private void DoExtraAttack()
    {
        Debug.Log("Extra Attack triggered in Phase 2!");
        // Add your effects, spawns, or mechanics here
    }

    private void DoHeavyAttack()
    {
        Debug.Log("Heavy Attack triggered in Phase 3!");
        // Add heavy area damage, lasers, spikes, etc.
    }
}
