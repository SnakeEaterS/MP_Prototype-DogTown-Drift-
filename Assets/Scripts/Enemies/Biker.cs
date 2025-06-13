using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biker : MonoBehaviour
{
    private EnemySpawner spawner;
    private PlayerHealth playerHealth;

    private int lane;
    private int bikerIndex;
    private bool hasStartedShooting = false;

    private Transform player;
    private Transform targetParent;
    private Transform target1, target2, target3, target4;
    private Transform target;

    public BikerShooting shootingController;  // assign in Inspector
    public Transform firePoint;               // assign in Inspector
    public LayerMask barrierLayer;            // assign in Inspector (Barrier layer)

    // Barrier avoidance
    private bool isAvoidingBarrier = false;
    private Vector3 avoidanceOffset = Vector3.zero;
    private float avoidDistance = 1.5f;
    private float avoidCooldown = 1f;

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerHealth>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (target == null)
        {
            Debug.LogError("Target not set for Biker! Please initialize the biker with a target before starting the game.");
            return;
        }

        DetectAndAvoidBarrier();

        float moveSpeed = 50f;
        Vector3 targetPos = target.position + avoidanceOffset;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (!hasStartedShooting && Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            hasStartedShooting = true;
            Invoke(nameof(StartAiming), 2f);
            Invoke(nameof(Shoot), 3f);
        }
    }

    void StartAiming()
    {
        if (shootingController != null)
        {
            shootingController.StartShootingBeam();
        }
    }

    void Shoot()
    {
        if (shootingController != null)
        {
            shootingController.StopShootingBeam();
        }

        if (player == null) return;

        Vector3 direction = (player.position - firePoint.position).normalized;
        Ray ray = new Ray(firePoint.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Player")))
        {
            Debug.Log("Biker hit the player!");
            playerHealth?.TakeDamage(20f);
        }
        else
        {
            Debug.Log("Biker missed.");
        }

        hasStartedShooting = false;
    }

    private void DetectAndAvoidBarrier()
    {
        Ray forwardRay = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);

        if (Physics.Raycast(forwardRay, out RaycastHit hit, 2f, barrierLayer))
        {
            if (!isAvoidingBarrier)
            {
                isAvoidingBarrier = true;

                float side = Random.value > 0.5f ? 1f : -1f;
                avoidanceOffset = new Vector3(side * avoidDistance, 0f, 0f);

                Invoke(nameof(ResetAvoidance), avoidCooldown);
            }
        }
    }

    private void ResetAvoidance()
    {
        isAvoidingBarrier = false;
        avoidanceOffset = Vector3.zero;
    }

    private void FindTargets()
    {
        targetParent = GameObject.Find("BikerTargets")?.transform;
        if (targetParent != null)
        {
            target1 = targetParent.Find("Target1");
            target2 = targetParent.Find("Target2");
            target3 = targetParent.Find("Target3");
            target4 = targetParent.Find("Target4");
        }
        else
        {
            Debug.LogError("BikerTargets parent not found! Please ensure it exists in the scene.");
        }
    }

    public void Initialize(EnemySpawner spawner, int lane, int bikerIndex)
    {
        this.spawner = spawner;
        this.lane = lane;
        this.bikerIndex = bikerIndex;

        FindTargets();

        if (lane == -1)
        {
            target = (bikerIndex == 0) ? target4 : target1;
        }
        else if (lane == 1)
        {
            target = (bikerIndex == 0) ? target3 : target2;
        }
    }

    private void OnDestroy()
    {
        if (spawner != null && spawner.bikers.ContainsKey(lane))
        {
            spawner.bikers[lane].Remove(gameObject);
            spawner.ReturnBikerIndex(lane, bikerIndex);
            Debug.Log($"Biker removed from lane {lane}. Remaining: {spawner.bikers[lane].Count}");
        }
    }
}
