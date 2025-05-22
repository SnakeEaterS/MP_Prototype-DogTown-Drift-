using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health;
    public float speed;
    public float score;

    public void TakeDamage(float damage)
    {
        if (health > 0)
        {
            health -= damage;
            Debug.Log(health);
            if (health <= 0)
            {
                GameManager.Instance.AddScore(score); // Add score
                Destroy(gameObject);
            }
        }
        else
        {
            GameManager.Instance.AddScore(score); // Add score
            Destroy(gameObject);
        }
    }
}
