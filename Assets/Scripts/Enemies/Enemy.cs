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

    private Renderer[] renderers;
    private Color[] originalColors;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        carEnemy = GetComponent<CarEnemyShooting>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Get all renderers and cache original colors
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        if (health <= 0f) return;

        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining: {health}");

        StartCoroutine(FlashRed());

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

    private IEnumerator FlashRed()
    {
        // Set all materials to red
        foreach (var renderer in renderers)
        {
            renderer.material.color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        // Revert back to original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }
}