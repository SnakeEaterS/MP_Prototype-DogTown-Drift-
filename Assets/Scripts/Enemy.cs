using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health;
    public float speed;
    public float score;
    private Transform player;
    private float fixedY;
    private float fixedX;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        fixedX = transform.position.x; // Record Z at start
        fixedY = transform.position.y;
    }

    void Update()
    {
        Vector3 targetPosition = player.position + player.forward * 5f;

        // Maintain fixed Y and Z
        targetPosition.y = fixedY;
        targetPosition.x = fixedX;

        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;
    }

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