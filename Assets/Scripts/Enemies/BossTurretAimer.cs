using UnityEngine;

public class BossTurretPitchOnly : MonoBehaviour
{
    [Header("Turret Parts")]
    public Transform[] gunPivots; // Guns that pitch on local X

    [Header("Target")]
    public Transform player; // Auto-find if empty

    [Header("Aiming")]
    public float pitchSpeed = 60f; // Degrees per second

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found) player = found.transform;
        }
    }

    void Update()
    {
        if (player == null || gunPivots.Length == 0) return;

        foreach (Transform gun in gunPivots)
        {
            if (gun == null) continue;

            // 1? Direction to player in local space
            Vector3 localDir = gun.InverseTransformPoint(player.position);

            // 2? If your gun faces -Z, invert Z when calculating pitch:
            // Instead of using localDir.z, use -localDir.z
            float desiredPitch = Mathf.Atan2(localDir.y, -localDir.z) * Mathf.Rad2Deg;

            // 3?Get current local X rotation
            Vector3 localEuler = gun.localEulerAngles;
            float currentPitch = localEuler.x;
            if (currentPitch > 180f) currentPitch -= 360f;

            // 4?Smooth towards target pitch
            float newPitch = Mathf.MoveTowards(currentPitch, desiredPitch, pitchSpeed * Time.deltaTime);

            gun.localEulerAngles = new Vector3(newPitch, localEuler.y, localEuler.z);
        }
    }
}
