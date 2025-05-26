using UnityEngine;

public class BarrierCollision : MonoBehaviour
{
    public float despawnDistance = 30f; // Distance behind the player to despawn
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found. Barrier won't despawn based on distance.");
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check if the barrier is far behind the player on the z-axis
        if (transform.position.z < player.position.z - despawnDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit the barrier!");
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(30f); // Adjust damage amount as needed
            }
            Destroy(gameObject);

        }
    }
}
