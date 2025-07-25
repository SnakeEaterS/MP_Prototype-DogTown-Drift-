using UnityEngine;
using System.Collections;
using UnityEngine.Splines;

public class BossAttacks : MonoBehaviour
{
    [Header("References")]
    public Transform player; // optional assign
    public BikeSplineFollower bikeSplineFollower; // auto found if null

    [Header("Warning Indicator Prefab")]
    public GameObject warningIndicatorPrefab;  // Assign the prefab here in Inspector
    private GameObject warningIndicatorInstance;

    [Header("Warning Settings")]
    public float warningDuration = 2f;
    public float warningRadius = 1f;
    public float warningYOffset = 0.1f; // Y axis offset for the warning indicator

    [Header("Attack Timing")]
    public float attackCooldown = 4f;

    [Header("Ground Raycast")]
    public float groundRaycastDistance = 10f;
    public float warningHeightOffset = 0.05f;

    [Header("Attack Damage")]
    public float strafeAttackDamage = 20f; // Set in Inspector

    [Header("Warning Follow Settings")]
    [Tooltip("How far ahead on the spline (in meters) to place the warning.")]
    public float forwardOffsetMeters = 5f;
    public float followSmoothing = 5f;

    private float cooldownTimer = 0f;
    private bool warningActive = false;
    private float warningTimer = 0f;

    [Header("Second Attack")]
    public bool startSecondAttack = false;
    public bool isSecondAttackRunning = false;

    public GameObject missilePrefab;
    public GameObject missile;
    public Transform missileSpawn;

    // Track if player is inside warning zone
    private bool playerInsideWarning = false;

    void Start()
    {
        // Find player transform if missing
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogError("BossAttacks: Player with tag 'Player' not found!");
        }

        // Find spline follower if missing
        if (bikeSplineFollower == null && player != null)
        {
            bikeSplineFollower = player.GetComponent<BikeSplineFollower>();
            if (bikeSplineFollower == null)
                bikeSplineFollower = player.GetComponentInChildren<BikeSplineFollower>();

            if (bikeSplineFollower == null)
                Debug.LogError("BossAttacks: BikeSplineFollower not found on player!");
        }

        if (warningIndicatorPrefab != null)
        {
            warningIndicatorInstance = Instantiate(warningIndicatorPrefab);
            warningIndicatorInstance.SetActive(false);

            WarningZone wz = warningIndicatorInstance.GetComponent<WarningZone>();
            if (wz != null)
            {
                wz.bossAttacks = this;
            }
            else
            {
                Debug.LogError("Warning Indicator Prefab needs a WarningZone component!");
            }
        }
        else
        {
            Debug.LogError("BossAttacks: Warning Indicator Prefab not assigned!");
        }
    }

    void Update()
    {
        if (player == null || bikeSplineFollower == null) return;

        CheckWings();

        if (!startSecondAttack)
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
        playerInsideWarning = false;

        if (warningIndicatorInstance != null)
        {
            warningIndicatorInstance.SetActive(true);
            UpdateWarningPosition();
        }
    }

    void UpdateWarningPosition()
    {
        if (warningIndicatorInstance == null || bikeSplineFollower == null) return;

        Vector3 currentPos = warningIndicatorInstance.transform.position;

        float splineLength = bikeSplineFollower.spline.CalculateLength();

        float tOffset = forwardOffsetMeters / splineLength;

        float t = bikeSplineFollower.GetSplineT() + tOffset;

        if (bikeSplineFollower.loopSpline)
        {
            t %= 1f;
        }
        else
        {
            t = Mathf.Clamp01(t);
        }

        Vector3 splinePoint = bikeSplineFollower.spline.EvaluatePosition(t);

        Vector3 tangent = ((Vector3)bikeSplineFollower.spline.EvaluateTangent(t)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        float playerOffset = bikeSplineFollower.GetHorizontalOffset();
        splinePoint += right * playerOffset;

        Vector3 rayOrigin = splinePoint + Vector3.up * 5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundRaycastDistance))
        {
            splinePoint.y = hit.point.y + warningHeightOffset + warningYOffset;
        }
        else
        {
            splinePoint.y += warningYOffset;
        }

        Vector3 smoothedPos = Vector3.Lerp(currentPos, splinePoint, Time.deltaTime * followSmoothing);
        warningIndicatorInstance.transform.position = smoothedPos;
    }


    void ExecuteAttack()
    {
        warningActive = false;

        if (warningIndicatorInstance != null)
            warningIndicatorInstance.SetActive(false);

        if (playerInsideWarning)
        {
            Debug.Log("Player hit by strafe attack!");

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(strafeAttackDamage);
                Debug.Log($"Dealt {strafeAttackDamage} damage to player.");
            }
            else
            {
                Debug.LogWarning("Player does not have a PlayerHealth component!");
            }
        }
        else
        {
            Debug.Log("Player dodged the attack!");
        }

        cooldownTimer = attackCooldown;
    }

    public void PlayerEnteredWarningZone()
    {
        playerInsideWarning = true;
        Debug.Log("Player entered warning zone!");
    }

    public void PlayerExitedWarningZone()
    {
        playerInsideWarning = false;
        Debug.Log("Player exited warning zone!");
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

        yield return new WaitForSeconds(1f);
        if (missilePrefab != null)
        {
            missile = Instantiate(missilePrefab, missileSpawn.position, Quaternion.identity);
            Debug.Log("Missile spawned.");
        }
    }
}
