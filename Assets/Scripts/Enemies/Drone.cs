using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    [Header("Position Offsets")]
    public Vector2 xRange = new Vector2(-3f, 3f);
    public Vector2 yRange = new Vector2(1f, 4f);
    public float zOffset = -5f;
    public float entryDistance = 20f;

    [Header("Movement & Hovering")]
    public float moveSpeed = 5f;
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 2f;

    [Header("Side Drift Settings")]
    public float sideDriftAmplitude = 1f;
    public float sideDriftFrequency = 1.5f;
    private float sideDriftOffset;

    [Header("Avoidance")]
    public float avoidRadius = 1.5f;
    public float avoidStrength = 2f;

    [Header("Aggression Settings")]
    public bool aggressive = false;
    public float attackSpeed = 10f;
    public float damageAmount = 20f;
    public Vector2 aggressionDelayRange = new Vector2(3f, 7f);
    public float missTimeout = 5f;

    [Header("Attack Target Offset")]
    public Vector3 attackOffset = new Vector3(0f, -1.0f, 0f); // NEW: flies lower when attacking

    private Transform player;
    private PlayerHealth playerHealth;
    private Vector3 localOffset;
    private Vector3 velocity = Vector3.zero;
    private bool attackStarted = false;
    private CameraShake cameraShake;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraShake = mainCam.GetComponent<CameraShake>();
            if (cameraShake == null)
                Debug.LogWarning("CameraShake component not found on main camera.");
        }

        player = playerObj.transform;
        playerHealth = player.GetComponent<PlayerHealth>();

        float randX = Random.Range(xRange.x, xRange.y);
        float randY = Random.Range(yRange.x, yRange.y); 
        localOffset = new Vector3(randX, randY, zOffset);

        int side = Random.value < 0.5f ? -1 : 1;
        Vector3 right = player.right * entryDistance * side;
        Vector3 entryWorldOffset = player.TransformPoint(localOffset) + right;

        transform.position = entryWorldOffset;

        // Get phase overrides if any
        SetMovementOverrides(
            GamePhaseManager.CurrentDroneHoverAmplitude,
            GamePhaseManager.CurrentDroneHoverFrequency,
            GamePhaseManager.CurrentDroneDriftAmplitude,
            GamePhaseManager.CurrentDroneDriftFrequency
        );

        sideDriftOffset = Random.Range(0f, Mathf.PI * 2f);

        float randomDelay = Random.Range(aggressionDelayRange.x, aggressionDelayRange.y);
        StartCoroutine(ActivateAggressionAfterDelay(randomDelay));
    }

    void Update()
    {
        if (player == null) return;

        if (aggressive)
        {
            if (!attackStarted)
            {
                attackStarted = true;
                StartCoroutine(SelfDestructAfterMiss(missTimeout));
            }

            Vector3 attackTarget = player.position + attackOffset;
            Vector3 direction = (attackTarget - transform.position).normalized;

            transform.position += direction * attackSpeed * Time.deltaTime;

            Vector3 flatDirection = (attackTarget - transform.position);
            flatDirection.y = 0;

            if (flatDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(flatDirection.normalized) * Quaternion.Euler(0, 90f, 0);

            return;
        }

        Vector3 flatDirectionNormal = (player.position - transform.position);
        flatDirectionNormal.y = 0;

        float hover = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        float sideDrift = Mathf.Sin(Time.time * sideDriftFrequency + sideDriftOffset) * sideDriftAmplitude;

        Vector3 localDriftedOffset = new Vector3(localOffset.x + sideDrift, localOffset.y + hover, localOffset.z);
        Vector3 targetPos = player.TransformPoint(localDriftedOffset);

        Vector3 avoidance = Vector3.zero;
        foreach (Drone other in FindObjectsOfType<Drone>())
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < avoidRadius)
            {
                Vector3 away = (transform.position - other.transform.position).normalized;
                float strength = (avoidRadius - dist) / avoidRadius;
                avoidance += away * strength * avoidStrength;
            }
        }

        Vector3 finalTarget = targetPos + avoidance;
        transform.position = Vector3.SmoothDamp(transform.position, finalTarget, ref velocity, 0.3f);

        if (flatDirectionNormal != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(flatDirectionNormal.normalized) * Quaternion.Euler(0, 90f, 0);
    }

    IEnumerator ActivateAggressionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        aggressive = true;
    }

    IEnumerator SelfDestructAfterMiss(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        Debug.Log("Drone missed the player and self-destructed.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!aggressive) return;

        if (other.CompareTag("Player"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("Drone hit player, damage applied.");
            }

            if (cameraShake != null)
            {
                cameraShake.TriggerShake(0.3f, 0.2f);
            }

            Destroy(gameObject);
        }
    }

    public void SetMovementOverrides(float hoverAmp, float hoverFreq, float driftAmp, float driftFreq)
    {
        hoverAmplitude = hoverAmp;
        hoverFrequency = hoverFreq;
        sideDriftAmplitude = driftAmp;
        sideDriftFrequency = driftFreq;
    }
}
