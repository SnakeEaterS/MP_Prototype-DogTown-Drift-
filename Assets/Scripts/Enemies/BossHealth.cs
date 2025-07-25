using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Stats")]
    public float health = 500f;
    public int score = 10;

    [Header("VFX & SFX")]
    public GameObject smokeVFXPrefab;
    public AudioClip deathSoundClip;
    public AudioSource audioSource;

    [Header("Scoreboard")]
    public GameObject scoreboard;

    [Header("References")]
    public GameManager gameManager;

    [Header("Damage Feedback")]
    public Material hitFlashMaterial; // Assign your flash material here
    public float flashDuration = 0.1f;

    private Renderer[] renderers;
    private Material[] originalMaterials;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        // Get all renderers in this object & children
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining: {health}");

        StartCoroutine(FlashHitMaterial());

        if (health <= 0f)
        {
            Die();
        }
    }

    private IEnumerator FlashHitMaterial()
    {
        if (hitFlashMaterial == null) yield break;

        // Swap to flash
        foreach (Renderer r in renderers)
        {
            r.material = hitFlashMaterial;
        }

        yield return new WaitForSeconds(flashDuration);

        // Revert
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = originalMaterials[i];
        }
    }

    protected virtual void Die()
    {
        if (smokeVFXPrefab != null)
        {
            GameObject explosion = Instantiate(smokeVFXPrefab, transform.position, Quaternion.identity);

            var followScript = explosion.AddComponent<FollowTargetTemporary>();
            followScript.target = this.transform;
            followScript.duration = 1000f;
            followScript.followRotation = true;
            followScript.rotationOffset = new Vector3(180f, 0f, 0f);
        }

        if (scoreboard != null)
        {
            scoreboard.SetActive(true);
            Destroy(scoreboard, 5f);
        }

        gameManager.AddScore(score);
        gameManager.partsDestroyed++;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (deathSoundClip != null && audioSource != null)
        {
            StartCoroutine(RemoveAfterDeath());
        }
        else
        {
            Destroy(this); // Fallback destroy
        }
    }

    private IEnumerator RemoveAfterDeath()
    {
        audioSource.PlayOneShot(deathSoundClip);

        yield return new WaitForSeconds(deathSoundClip.length);

        Destroy(this);
    }
}
