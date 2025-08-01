using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEnemy : MonoBehaviour
{
    private EnemySpawner spawner;
    private PlayerHealth playerHealth;

    private int lane;
    private int bikerIndex;
    private bool hasStartedShooting = false;
    private bool hasMovedToFront = false;

    private Transform player;
    private Transform targetParent;
    private Transform target1, target2, target3, target4;
    private Transform target;

    public CarEnemyShooting shootingController;  // assign in Inspector
    public Transform firePoint1;                 // assign in Inspector
    public Transform firePoint2;                 // assign in Inspector
    public LayerMask barrierLayer;               // assign in Inspector (Barrier layer)
    public AudioSource audioSource; // assign in Inspector (optional for sound effects)
    public AudioClip shootSound; // assign in Inspector (optional for shooting sound)

    // Barrier avoidance
    private bool isAvoidingBarrier = false;
    private Vector3 avoidanceOffset = Vector3.zero;
    private float avoidDistance = 1.5f;
    private float avoidCooldown = 1f;
    private float chooseFirePoint = 0f;
    private Transform firePoint;

    private bool isPushingPlayer = false;
    private JoyconRevController control;
    private float pushSpeed = 5f;
    private float pushDirection = 0f;
    private bool carDamaged = false;

    public Vector3 offset = new Vector3(0,0,0);
    private float offsetZTimer = 0f;
    private float offsetZInterval = 2f; // 1 second interval
    private float currentOffsetZ = 0f;
    private float targetOffsetZ = 0f;
    private float offsetSmoothSpeed = 1f;
    private float moveSpeed = 50f;

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

        if (isPushingPlayer && control != null)
        {
            float currentOffset = control.currentOffset; // You might need to expose this in your BikeSplineFollower
            float newOffset = currentOffset + pushDirection * pushSpeed * Time.deltaTime;
            newOffset = Mathf.Clamp(newOffset, -control.maxHorizontalOffset, control.maxHorizontalOffset);
            control.CarCollision(newOffset);
        }

        TryMoveToFrontPosition();
        DetectAndAvoidBarrier();

        // Smoothly interpolate the Z offset
        currentOffsetZ = Mathf.Lerp(currentOffsetZ, targetOffsetZ, Time.deltaTime * offsetSmoothSpeed);
        offset = new Vector3(0, 0, currentOffsetZ);

        // Randomize target offset every interval
        offsetZTimer += Time.deltaTime;
        if (offsetZTimer >= offsetZInterval)
        {
            targetOffsetZ = Random.Range(-1, 1f);
            offsetZTimer = 0f;
        }

        if (transform.position == target.position + avoidanceOffset + offset)
        {
            moveSpeed = 10f;
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * 5f);
            return;
        }

        Vector3 targetPos = target.position + avoidanceOffset + offset;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * 5f);

        if (!hasStartedShooting && Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            hasStartedShooting = true;

            if (firePoint1 == null || firePoint2 == null)
            {
                Debug.LogError("Fire points not assigned! Please assign fire points in the Inspector.");
                return;
            }

            firePoint = (chooseFirePoint == 1f) ? firePoint1 : firePoint2;
            chooseFirePoint = 1f - chooseFirePoint; // Toggle between 0 and 1

            Invoke(nameof(StartAiming), 2f);
            Invoke(nameof(Shoot), 3f);
        }
    }

    void StartAiming()
    {
        shootingController?.StartShootingBeam(firePoint);
    }

    void Shoot()
    {
        shootingController?.StopShootingBeam();
        if (player == null) return;

        Vector3 direction = (player.position - firePoint.position).normalized;
        Ray ray = new Ray(firePoint.position, direction);
        audioSource?.PlayOneShot(shootSound);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Player")))
        {
            Debug.Log("Car Enemy hit the player!");
            playerHealth?.TakeDamage(20f);
        }
        else
        {
            Debug.Log("Car Enemy missed.");
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
            target = (bikerIndex == 0) ? target4 : target1;
        else if (lane == 1)
            target = (bikerIndex == 0) ? target3 : target2;
    }

    private void TryMoveToFrontPosition()
    {
        if (hasMovedToFront || bikerIndex != 1 || spawner == null) return;

        int frontIndex = 0;
        var laneCars = spawner.cars.ContainsKey(lane) ? spawner.cars[lane] : null;
        if (laneCars == null) return;

        foreach (var carObj in laneCars)
        {
            if (carObj == null) continue;
            var carScript = carObj.GetComponent<CarEnemy>();
            if (carScript != null && carScript.bikerIndex == frontIndex)
                return; // Spot already taken
        }

        spawner.ReturnCarIndex(lane, bikerIndex); // Return old index
        bikerIndex = frontIndex;
        hasMovedToFront = true;
        spawner.ReserveCarIndex(lane, frontIndex);

        if (lane == -1) target = target4;
        else if (lane == 1) target = target3;

        Debug.Log($"[CarEnemy] Car on lane {lane} moved to front (index 0)");
    }

    private void OnDestroy()
    {
        if (spawner != null && spawner.cars.ContainsKey(lane))
        {
            spawner.cars[lane].Remove(gameObject);
            spawner.ReturnCarIndex(lane, bikerIndex);
            Debug.Log($"Car Enemy removed from lane {lane}. Remaining: {spawner.cars[lane].Count}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Car started pushing the player!");
            if (carDamaged == false)
            {
                playerHealth?.TakeDamage(20f);
                carDamaged = true;
                other.GetComponentInChildren<CameraShake>()?.TriggerShake(1f, 1f);
            }

            control = other.GetComponent<JoyconRevController>();
            if (control != null)
            {
                isPushingPlayer = true;

                // Determine which direction to push: lane -1 (left) => push right (1), lane 1 => push left (-1)
                pushDirection = (lane == -1) ? 1f : -1f;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Car stopped pushing the player!");
            isPushingPlayer = false;
            carDamaged = false;
            control = null;
        }
    }
}
    