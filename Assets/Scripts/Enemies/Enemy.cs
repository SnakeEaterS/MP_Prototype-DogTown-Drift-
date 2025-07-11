using UnityEngine;
using System.Collections; // Required for IEnumerator

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public int score = 10;
    public GameObject explosionVFXPrefab; // Assign your particle system prefab in the Inspector
    public GameObject scoreboard; // Assign your scoreboard UI GameObject here in the Inspector
    public AudioClip deathSoundClip;
    public GameObject car;
    public GameObject turret;

    public AudioSource audioSource;
    public CarEnemyShooting carEnemy;

    void Awake()
    {
        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        carEnemy = GetComponent<CarEnemyShooting>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

    }

    public void TakeDamage(float damage)
    {
        if (health <= 0f) return;

        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining: {health}");

        if (health <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {

        if (explosionVFXPrefab != null)
        {
            GameObject explosion = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

            // Add follow behavior without parenting
            var followScript = explosion.AddComponent<FollowTargetTemporary>();
            followScript.target = this.transform; // Enemy
            followScript.duration = 2f; // Match explosion VFX length
        }

        // Add score
        GameManager.Instance.AddScore(score);

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child != transform) // Optional: skip the parent object itself
            {
                child.gameObject.SetActive(false);
            }
        }

        // Optionally disable all colliders to prevent interaction
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        // Disable logic-related scripts like BikerShooting
        if (carEnemy != null)
        {
            carEnemy.enabled = false;
        }

        // Show scoreboard if available
        if (scoreboard != null)
        {
            scoreboard.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Scoreboard GameObject not assigned to Enemy script!");
        }

        // Play death sound and destroy
        if (deathSoundClip != null && audioSource != null)
        {
            StartCoroutine(PlayDeathSoundAndDestroy());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator PlayDeathSoundAndDestroy()
    {
        audioSource.PlayOneShot(deathSoundClip);
        yield return new WaitForSeconds(deathSoundClip.length);
        Destroy(gameObject);
    }
}