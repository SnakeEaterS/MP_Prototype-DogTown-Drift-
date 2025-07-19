using UnityEngine;

public class TurretAimer : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // Will be auto-found if null

    [Header("Turret Parts")]
    public Transform turretBase;     // Rotates Y-axis
    public Transform[] gunBarrels;   // Rotates X-axis

    [Header("Rotation Speeds")]
    public float baseTurnSpeed = 5f;   // Yaw
    public float barrelTurnSpeed = 5f; // Pitch

    [Header("Pitch Limits")]
    public float minPitch = -5f;  // Down limit
    public float maxPitch = 45f;  // Up limit

    void Start()
    {
        // Try to find player automatically by tag if not set
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("MainCamera");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogWarning($"[{name}] No player found with tag 'Player'. Turret will not aim.");
            }
        }
    }

    void Update()
    {
        if (player == null || turretBase == null || gunBarrels.Length == 0) return;

        Vector3 targetPos = player.position;

        // --- Rotate Base (Y) ---
        Vector3 baseDirection = targetPos - turretBase.position;
        baseDirection.y = 0f;

        if (baseDirection.sqrMagnitude > 0.001f)
        {
            Quaternion desiredBaseRotation = Quaternion.LookRotation(baseDirection);
            turretBase.rotation = Quaternion.Slerp(
                turretBase.rotation,
                desiredBaseRotation,
                Time.deltaTime * baseTurnSpeed
            );
        }

        // --- Calculate desired pitch ---
        Vector3 localTarget = turretBase.InverseTransformPoint(targetPos);
        float desiredPitch = -Mathf.Atan2(localTarget.y, localTarget.z) * Mathf.Rad2Deg;
        desiredPitch = Mathf.Clamp(desiredPitch, minPitch, maxPitch);

        // --- Apply pitch to all barrels ---
        foreach (Transform barrel in gunBarrels)
        {
            Vector3 barrelEuler = barrel.localEulerAngles;
            if (barrelEuler.x > 180f) barrelEuler.x -= 360f;

            barrelEuler.x = Mathf.Lerp(barrelEuler.x, desiredPitch, Time.deltaTime * barrelTurnSpeed);
            barrel.localEulerAngles = new Vector3(barrelEuler.x, 0f, 0f);
        }
    }
}
