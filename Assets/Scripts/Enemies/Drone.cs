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

    [Header("Avoidance")]
    public float avoidRadius = 1.5f;
    public float avoidStrength = 2f;

    [Header("Aggression Settings")]
    public bool aggressive = false;
    public float attackSpeed = 10f;
    public float damageAmount = 20f;
    public Vector2 aggressionDelayRange = new Vector2(3f, 7f);
    public float missTimeout = 5f; // Time after aggression to self-destruct if miss

    private Transform player;
    private PlayerHealth playerHealth;
    private Vector3 localOffset;
    private Vector3 velocity = Vector3.zero;
    private bool attackStarted = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        player = playerObj.transform;
        playerHealth = player.GetComponent<PlayerHealth>();

        float randX = Random.Range(xRange.x, xRange.y);
        float randY = Random.Range(yRange.y, yRange.y);
        localOffset = new Vector3(randX, randY, zOffset);

        int side = Random.value < 0.5f ? -1 : 1;
        Vector3 right = player.right * entryDistance * side;
        Vector3 entryWorldOffset = player.TransformPoint(localOffset) + right;

        transform.position = entryWorldOffset;

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

            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * attackSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);

            // No more manual distance collision check here.
            return;
        }

        // Idle hover behavior
        float hover = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        Vector3 targetPos = player.TransformPoint(localOffset) + new Vector3(0, hover, 0);

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
        transform.rotation = Quaternion.LookRotation(player.forward);
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
        if (!aggressive) return;  // Only damage when aggressive

        if (other.CompareTag("Player"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("Drone hit player, damage applied.");
            }
            Destroy(gameObject);
        }
    }
}
