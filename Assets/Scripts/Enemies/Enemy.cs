using UnityEngine;
using System.Collections; // Required for IEnumerator

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public float score = 10f;
    public GameObject scoreboard; // Assign your scoreboard UI GameObject here in the Inspector
    public AudioClip deathSoundClip;
    public GameObject head;

    public AudioSource audioSource;
    public MeshRenderer meshRenderer; // Reference to the MeshRenderer
    public BikerShooting biker;

    void Awake()
    {
        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        biker = GetComponent<BikerShooting>();
        head = transform.Find("Head")?.gameObject;
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Get the MeshRenderer component
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning($"No MeshRenderer found on {gameObject.name}. Visual disappearance on death might not work as expected.");
        }


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
        // Add score immediately
        GameManager.Instance.AddScore(score);

        // Deactivate visuals and collision immediately
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        if (biker != null)
        {
            biker.enabled = false; // Disable the biker script to stop any further movement or actions
        }

        if (head != null)
        {
            head.SetActive(false); // Hide the head GameObject
        }

        // Ensure the scoreboard is activated before potentially destroying the object
        if (scoreboard != null)
        {
            scoreboard.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Scoreboard GameObject not assigned to Enemy script!");
        }

        // If there's a death sound, play it and then destroy after it finishes
        if (deathSoundClip != null && audioSource != null)
        {
            StartCoroutine(PlayDeathSoundAndDestroy());
        }
        else
        {
            // If no sound, just destroy the object immediately
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