using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float healDelay = 3f;
    public float healRate = 10f;

    public Image lowHealthImage; // Image to fade

    private float lastHitTime;
    private bool isDead = false;

    public bool IsInvulnerable { get; set; } = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (lowHealthImage != null)
        {
            Color c = lowHealthImage.color;
            c.a = 0f;
            lowHealthImage.color = c;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (Time.time - lastHitTime > healDelay && currentHealth < maxHealth)
        {
            currentHealth += healRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        if (lowHealthImage != null)
        {
            float targetAlpha = Mathf.InverseLerp(90f, 0f, currentHealth); // More alpha as health drops
            Color currentColor = lowHealthImage.color;
            lowHealthImage.DOFade(targetAlpha, 0.3f);
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

        if (lowHealthImage != null)
            DOTween.Kill(lowHealthImage); // Kill tweens targeting the image

        Debug.LogError("Player has died!");
        GameManager.Instance.PlayerDie();
    }

}
