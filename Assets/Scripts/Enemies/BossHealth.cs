using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float health = 500f; // Boss health
    public int score = 10;
    public GameObject smokeVFXPrefab; // Assign your particle system prefab in the Inspector
    public AudioClip deathSoundClip; // Sound to play on death
    public AudioSource audioSource; // AudioSource to play sounds
    public GameObject scoreboard; // Assign your scoreboard UI GameObject here in the Inspector
    public GameManager gameManager; // Reference to the GameManager

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        if (gameManager == null)
        {
            gameManager = GameManager.Instance; // Get the GameManager instance
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining: {health}");

        if (health <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {

        if (smokeVFXPrefab != null)
        {
            GameObject explosion = Instantiate(smokeVFXPrefab, transform.position, Quaternion.identity);

            // Add follow behavior without parenting
            var followScript = explosion.AddComponent<FollowTargetTemporary>();
            followScript.target = this.transform; // Enemy
            followScript.duration = 100f; // Match explosion VFX length
        }

        if (scoreboard != null)
        {
            scoreboard.SetActive(true); // Show scoreboard
            Destroy(scoreboard, 5f); // Destroy scoreboard after 5 seconds
        }

        // Add score
        GameManager.Instance.AddScore(score);

        if (gameObject.GetComponent<Collider>() != null)
        {
            gameObject.GetComponent<Collider>().enabled = false; // Disable collider
        }

        // Play death sound and destroy
        if (deathSoundClip != null && audioSource != null)
        {
            StartCoroutine(PlayDeathSound());
        }

        gameManager.partsDestroyed++; // Increment parts destroyed counter
        this.enabled = false;

    }
    private IEnumerator PlayDeathSound()
    {
        audioSource.clip = deathSoundClip;
        audioSource.Play();
        yield return new WaitForSeconds(deathSoundClip.length);
    }
}
