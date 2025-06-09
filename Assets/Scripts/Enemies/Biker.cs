using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Biker : MonoBehaviour
{
    private EnemySpawner spawner;
    private PlayerHealth playerHealth;
    private int lane;
    private int bikerIndex;

    private float catchUpSpeed = 10f;
    private float laneOffsetX;
    private float targetLocalZ;

    private bool hasStartedShooting = false;

    public BikerShooting shootingController;  // assign in Inspector
    public Transform firePoint;    // Assign in Inspector (position to shoot from)

    private Transform player;


    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerHealth>();
        laneOffsetX = transform.localPosition.x;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        Vector3 localPos = transform.localPosition;

        localPos.z = Mathf.MoveTowards(localPos.z, targetLocalZ, catchUpSpeed * Time.deltaTime);
        localPos.x = laneOffsetX;
        localPos.y = 1f;

        transform.localPosition = localPos;

        if (!hasStartedShooting && Mathf.Abs(transform.localPosition.z - targetLocalZ) < 0.01f)
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

    public void Initialize(EnemySpawner spawner, int lane, int bikerIndex)
    {
        this.spawner = spawner;
        this.lane = lane;
        this.bikerIndex = bikerIndex;

        targetLocalZ = 7f + bikerIndex * -5f;
    }

    private void OnDestroy()
    {
        if (spawner != null && spawner.bikers.ContainsKey(lane))
        {
            spawner.bikers[lane].Remove(gameObject);
        }
    }
}