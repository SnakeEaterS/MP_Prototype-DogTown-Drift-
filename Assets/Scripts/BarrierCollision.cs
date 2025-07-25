using UnityEngine;

public class BarrierCollision : MonoBehaviour
{
    public float despawnDistance = 30f;
    private Transform player;

    private Joycon playerJoycon;
    private CameraShake cameraShake;

    [Header("SFX")]
    public AudioSource PlayerAS;
    public AudioSource CarAS;
    public AudioClip CollisionClip;


    [Header("Explosion VFX")]
    public GameObject explosionVFXPrefab; // Assign a VFX prefab here

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
            
            PlayerAS = other.GetComponent<AudioSource>();

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

            PlayerAS.PlayOneShot(CollisionClip);

            Destroy(gameObject);
        }
        else if (other.CompareTag("EnemyCar"))
        {
            CarAS = other.GetComponent<AudioSource>();

            Debug.Log("Enemy car hit the barrier!");

            // Spawn explosion VFX at the barrier's position
            if (explosionVFXPrefab != null)
            {
                Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Explosion VFX Prefab not assigned!");
            }

            CarAS.PlayOneShot(CollisionClip);

            Destroy(gameObject);
        }
    }
}
