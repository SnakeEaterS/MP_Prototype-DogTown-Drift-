using UnityEngine;

public class BarrierCollision : MonoBehaviour
{
    public float despawnDistance = 30f;
    private Transform player;

    private Joycon playerJoycon;
    private CameraShake cameraShake;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found. Barrier won't despawn based on distance.");
        }

        var joycons = JoyconManager.Instance.j;
        if (joycons != null && joycons.Count > 0)
        {
            playerJoycon = joycons[0];
        }

        // Find CameraShake script on the main camera
        if (Camera.main != null)
        {
            cameraShake = Camera.main.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                Debug.LogWarning("CameraShake component not found on Main Camera.");
            }
        }
    }

    void Update()
    {
        if (player == null) return;

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

            // Trigger Joycon rumble
            if (playerJoycon != null)
            {
                playerJoycon.SetRumble(150, 300, 0.5f);
            }

            // Trigger camera shake
            if (cameraShake != null)
            {
                cameraShake.TriggerShake(0.5f, 0.15f);
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(30f);
            }

            Destroy(gameObject);
        }
    }
}
