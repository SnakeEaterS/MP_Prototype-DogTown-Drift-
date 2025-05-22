using UnityEngine;

public class BarrierCollision : MonoBehaviour
{
    public float lifetime = 5f; // Time before the barrier auto-destroys

    void Start()
    {
        // Destroy the barrier after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit the barrier!");
            Destroy(gameObject);
        }
    }
}
