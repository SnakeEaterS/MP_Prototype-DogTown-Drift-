using UnityEngine;
public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public float score = 10f;

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
        GameManager.Instance.AddScore(score);
        Destroy(gameObject);
    }
}