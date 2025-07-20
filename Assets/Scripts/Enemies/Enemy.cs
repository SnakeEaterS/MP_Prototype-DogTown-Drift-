using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public int score = 10;
    public GameObject explosionVFXPrefab; // Assign your particle system prefab
    public GameObject scoreboard; // Scoreboard UI GameObject
    public AudioClip deathSoundClip;
    public GameObject car;
    public GameObject turret;

    public AudioSource audioSource;
    public CarEnemyShooting carEnemy;

    [Header("Hit Flash Settings")]
    public Material flashMaterial; // ?? Assign your flash material in the Inspector
    public float flashDuration = 0.1f;

    private Renderer[] renderers;
    private Material[][] originalMaterials; // Stores all original materials per renderer

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        carEnemy = GetComponent<CarEnemyShooting>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Cache all renderers
        renderers = GetComponentsInChildren<Renderer>();

        // Store each renderer's original materials
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].materials;
        }
    }

    public void TakeDamage(float damage)
    {
        if (health <= 0f) return;

        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining: {health}");

        StartCoroutine(FlashHitMaterial());

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

            var followScript = explosion.AddComponent<FollowTargetTemporary>();
            followScript.target = this.transform; // Enemy
            followScript.duration = 2f; // Match explosion VFX length
        }

        GameManager.Instance.AddScore(score);

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child != transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        if (carEnemy != null)
        {
            carEnemy.enabled = false;
        }

        if (scoreboard != null)
        {
            scoreboard.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Scoreboard GameObject not assigned!");
        }

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

    private IEnumerator FlashHitMaterial()
    {
        if (flashMaterial == null)
        {
            Debug.LogWarning("Flash Material is not assigned!");
            yield break;
        }

        // Replace all materials with flash material
        foreach (Renderer rend in renderers)
        {
            Material[] flashMats = new Material[rend.materials.Length];
            for (int i = 0; i < flashMats.Length; i++)
            {
                flashMats[i] = flashMaterial;
            }
            rend.materials = flashMats;
        }

        yield return new WaitForSeconds(flashDuration);

        // Restore originals
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].materials = originalMaterials[i];
        }
    }
}
