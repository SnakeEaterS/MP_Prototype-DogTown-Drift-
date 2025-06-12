using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float healDelay = 3f; // Seconds to wait before healing starts
    public float healRate = 10f; // Health per second

    public GameObject lowHealthUI; // Reference to your low health UI GameObject

    private float lastHitTime;
    private bool isDead = false;

    // ?? Invulnerability flag
    public bool IsInvulnerable { get; set; } = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (lowHealthUI != null)
            lowHealthUI.SetActive(false); // Ensure it's hidden at start
    }

    void Update()
    {
        if (isDead) return;

        if (Time.time - lastHitTime > healDelay && currentHealth < maxHealth)
        {
            currentHealth += healRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        if (lowHealthUI != null)
        {
            lowHealthUI.SetActive(currentHealth < 20f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || IsInvulnerable) return;

        currentHealth -= amount;
        lastHitTime = Time.time;

        Debug.LogWarning($"Player took {amount} damage! Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.LogError("Player has died!");
        SceneManager.LoadScene(3);
    }
}
