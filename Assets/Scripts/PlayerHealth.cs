using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float healDelay = 3f; // Seconds to wait before healing starts
    public float healRate = 10f; // Health per second

    private float lastHitTime;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        // Start healing if enough time has passed since last hit
        if (Time.time - lastHitTime > healDelay && currentHealth < maxHealth)
        {
            currentHealth += healRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

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
        // You can add more logic here like disabling controls, triggering animations, etc.
    }
}
