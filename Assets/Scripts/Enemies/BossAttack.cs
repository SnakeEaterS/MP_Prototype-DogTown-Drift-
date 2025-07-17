using UnityEngine;
using System.Collections;

public class BossAttacks : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Warning Indicator Prefab")]
    public GameObject warningIndicatorPrefab;  // Assign the prefab here in Inspector

    private GameObject warningIndicatorInstance;

    [Header("Warning Settings")]
    public float warningDuration = 2f;
    public float warningRadius = 1f;
    public float warningYOffset = 0.1f;         // Y axis offset for the warning indicator

    [Header("Attack Timing")]
    public float attackCooldown = 4f;

    [Header("Ground Raycast")]
    public float groundRaycastDistance = 10f;
    public float warningHeightOffset = 0.05f;

    [Header("Warning Follow Settings")]
    public float followSpeed = 3f;               // Speed to follow player's Z position

    private float cooldownTimer = 0f;
    private bool warningActive = false;
    private float warningTimer = 0f;

    [Header("Second Attack")]
    public bool startSecondAttack = false;
    public bool isSecondAttackRunning = false;
    public bool spawnMissile = false;
    public float backOffDistance = 5f;
    public float backOffSpeed = 5f;

    public GameObject missilePrefab;
    public GameObject missile;

    public Transform missileSpawn;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogError("BossAttacks: Player with tag 'Player' not found!");
        }

        if (warningIndicatorPrefab != null)
        {
            // Instantiate once at start and disable
            warningIndicatorInstance = Instantiate(warningIndicatorPrefab);
            warningIndicatorInstance.SetActive(false);
        }
        else
        {
            Debug.LogError("BossAttacks: Warning Indicator Prefab not assigned!");
        }
    }

    void Update()
    {
        if (player == null) return;

        CheckWings();

        if (startSecondAttack == false) 
        {
            if (cooldownTimer <= 0f && !warningActive)
            {
                StartWarning();
            }

            if (warningActive)
            {
                warningTimer -= Time.deltaTime;

                if (warningTimer <= 0f)
                {
                    ExecuteAttack();
                }
                else
                {
                    UpdateWarningPosition();
                }
            }
            else
            {
                cooldownTimer -= Time.deltaTime;
            }
        }
        else
        {
            if (!isSecondAttackRunning)
            {
               Debug.Log("Starting second attack sequence.");
               StartCoroutine(SecondAttackSequence());
            }
        }
    }

    void StartWarning()
    {
        warningActive = true;
        warningTimer = warningDuration;

        if (warningIndicatorInstance != null)
        {
            warningIndicatorInstance.SetActive(true);
            UpdateWarningPosition();
        }
    }

    void UpdateWarningPosition()
    {
        if (warningIndicatorInstance == null) return;

        Vector3 indicatorPos = warningIndicatorInstance.transform.position; // current position

        // Raycast down from player to find ground X and Y position
        if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, groundRaycastDistance))
        {
            // Use hit point's X and Y (plus offsets)
            indicatorPos.x = hit.point.x;
            indicatorPos.y = hit.point.y + warningHeightOffset + warningYOffset;
        }
        else
        {
            // fallback: use player's X and Y with offset
            indicatorPos.x = player.position.x;
            indicatorPos.y = player.position.y - 1.5f + warningYOffset;
        }

        // Smoothly follow player Z position only
        indicatorPos.z = Mathf.Lerp(indicatorPos.z, player.position.z, Time.deltaTime * followSpeed);

        warningIndicatorInstance.transform.position = indicatorPos;
    }

    void ExecuteAttack()
    {
        warningActive = false;

        if (warningIndicatorInstance != null)
            warningIndicatorInstance.SetActive(false);

        float distance = Vector3.Distance(player.position, warningIndicatorInstance.transform.position);
        if (distance <= warningRadius)
        {
            Debug.Log("Player hit by strafe attack!");
            // TODO: Deal damage here
        }
        else
        {
            Debug.Log("Player dodged the attack!");
        }

        cooldownTimer = attackCooldown;
    }

    public void CheckWings()
    {
        BossHealth[] wings = GetComponentsInChildren<BossHealth>();
        if (wings.Length == 1)
        {
            startSecondAttack = true;
        }
    }

    private IEnumerator SecondAttackSequence()
    {
        isSecondAttackRunning = true;

        if (missilePrefab != null)
        {
            missile = Instantiate(missilePrefab, missileSpawn.position, Quaternion.identity);
            Debug.Log("Missile spawned.");
        }
        yield return new WaitForSeconds(2f); // Wait before charging
    }

}
