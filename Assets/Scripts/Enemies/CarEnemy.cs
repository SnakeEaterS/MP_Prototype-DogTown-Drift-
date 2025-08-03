using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEnemy : MonoBehaviour
{
    private EnemySpawner spawner;
    private PlayerHealth playerHealth;

    private int lane;
    private int carIndex;
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
            Debug.LogError("Target not set for car! Please initialize the car with a target before starting the game.");
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

        if (transform.position == target.position + offset)
        {
            moveSpeed = 10f;
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * 5f);
            return;
        }

        Vector3 targetPos = target.position + offset;
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

    private void FindTargets()
    {
        targetParent = GameObject.Find("EnemyTargets")?.transform;
        if (targetParent != null)
        {
            target1 = targetParent.Find("Target1");
            target2 = targetParent.Find("Target2");
            target3 = targetParent.Find("Target3");
            target4 = targetParent.Find("Target4");
        }
        else
        {
            Debug.LogError("EnemyTargets parent not found! Please ensure it exists in the scene.");
        }
    }

    public void Initialize(EnemySpawner spawner, int lane, int carIndex)
    {
        this.spawner = spawner;
        this.lane = lane;
        this.carIndex = carIndex;

        FindTargets();

        if (lane == -1)
            target = (carIndex == 0) ? target4 : target1;
        else if (lane == 1)
            target = (carIndex == 0) ? target3 : target2;
    }

    private void TryMoveToFrontPosition()
    {
        if (hasMovedToFront || carIndex != 1 || spawner == null) return;

        int frontIndex = 0;
        var laneCars = spawner.cars.ContainsKey(lane) ? spawner.cars[lane] : null;
        if (laneCars == null) return;

        foreach (var carObj in laneCars)
        {
            if (carObj == null) continue;
            var carScript = carObj.GetComponent<CarEnemy>();
            if (carScript != null && carScript.carIndex == frontIndex)
                return; // Spot already taken
        }

        spawner.ReturnCarIndex(lane, carIndex); // Return old index
        carIndex = frontIndex;
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
            spawner.ReturnCarIndex(lane, carIndex);
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
    