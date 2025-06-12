using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    public Transform firePoint;    // Assign in Inspector (position to shoot from)


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
        float moveSpeed = 50f; // customize as needed

        // Smoothly move toward the target's world position
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Once close enough, start shooting
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
            playerHealth?.TakeDamage(20f); // Adjust damage as needed
        }
        else
        {
            Debug.Log("Biker missed.");
        }
        hasStartedShooting = false; // Reset for next shoot cycle
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

        // Assign target based on lane and index
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
            spawner.ReturnBikerIndex(lane, bikerIndex);  // Notify spawner
            Debug.Log($"Biker removed from lane {lane}. Remaining: {spawner.bikers[lane].Count}");
        }
    }
}