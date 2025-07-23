using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public Transform firePoint;
    public Transform gunFire;
    public List<Transform> bulletSpawnPoints; // Assign in Inspector: bottom to top

    public GameObject bulletVisualPrefab;
    public GameObject explosionPrefab;
    public GameObject gunHand;
    public RectTransform crosshairUI;
    public Canvas canvas;

    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    public float damage = 10f;
    public float range = 100f;
    public float reloadTime = 2f;
    public float fireRate = 0.1f; // time between shots
    public int maxAmmo = 30;
    public int ammo; // Current ammo count

    public LayerMask shootableLayer;

    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    private Quaternion originalGunRotation;
    private List<GameObject> spawnedBullets = new List<GameObject>();

    private void Start()
    {
        ammo = maxAmmo;
        originalGunRotation = gunHand.transform.localRotation;
        SpawnAllBullets();
    }

    void Update()
    {
        if (isReloading) return;

        if (ammo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            if (ammo > 0)
            {
                nextTimeToFire = Time.time + fireRate;
                Debug.Log($"[Shooting] Ammo left: {ammo}");
                Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && ammo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        ammo--;

        // Audio
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Raycast
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position);
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, range, shootableLayer))
        {
            float finalDamage = damage;

            if (hit.collider.CompareTag("Enemy"))
            {
                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null) enemy.TakeDamage(finalDamage);
            }
            else if (hit.collider.CompareTag("LeftWing") || hit.collider.CompareTag("RightWing"))
            {
                BossHealth boss = hit.collider.GetComponent<BossHealth>();
                if (boss != null) boss.TakeDamage(finalDamage);
            }
            else if (hit.collider.CompareTag("Missile"))
            {
                MissileHealth missile = hit.collider.GetComponent<MissileHealth>();
                if (missile != null) missile.TakeDamage(finalDamage);
            }
            else
            {
                Debug.Log($"[Shooting] Hit {hit.collider.name} but no damage applied.");
            }
        }

        // Explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, gunFire.position, gunFire.rotation);
            var followScript = explosion.AddComponent<FollowTargetTemporary>();
            followScript.target = gunFire.transform;
            followScript.duration = 2f;
        }

        UpdateBulletVisuals();
    }

    void UpdateBulletVisuals()
    {
        if (spawnedBullets.Count == 0) return;

        int lastIndex = spawnedBullets.Count - 1;

        Destroy(spawnedBullets[lastIndex]);
        spawnedBullets.RemoveAt(lastIndex);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        // Tilt gun down
        Quaternion tiltRotation = Quaternion.Euler(0f, 0f, 30f); // Adjust the angle if needed
        gunHand.transform.localRotation = tiltRotation;

        yield return new WaitForSeconds(reloadTime);

        // Reset rotation
        gunHand.transform.localRotation = originalGunRotation;

        ammo = maxAmmo;
        SpawnAllBullets();
        isReloading = false;

        Debug.Log("Reload complete");
    }

    void SpawnAllBullets()
    {
        ClearAllBullets();

        for (int i = 0; i < bulletSpawnPoints.Count; i++)
        {
            GameObject bullet = Instantiate(bulletVisualPrefab, bulletSpawnPoints[i].position, Quaternion.Euler(90f, 0f, 0f)
, bulletSpawnPoints[i]);
            spawnedBullets.Add(bullet);
            Debug.Log($"[Shooting] Spawned bullet at {bulletSpawnPoints[i].position}");
        }
    }

    void ClearAllBullets()
    {
        foreach (GameObject bullet in spawnedBullets)
        {
            if (bullet != null)
                Destroy(bullet);
        }
        spawnedBullets.Clear();
    }
}
