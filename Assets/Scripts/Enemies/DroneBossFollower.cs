using UnityEngine;

public class DroneBossFollower : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform player;  // Will auto-search if null

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float switchInterval = 3f; // Time between picking new offset
    public float hoverHeight = 5f;    // Base hover height

    [Header("Offsets")]
    public Vector3 frontOffset = new Vector3(0, 0, 5f);
    public Vector3 leftOffset = new Vector3(-5f, 0, 0);
    public Vector3 rightOffset = new Vector3(5f, 0, 0);

    [Header("Hover Motion")]
    public float bobAmplitude = 0.5f;  // Up/down bob height
    public float bobFrequency = 1.5f;  // How fast to bob

    public float swayAmplitude = 0.5f; // Side sway range
    public float swayFrequency = 0.8f;

    public float driftAmplitude = 0.5f; // Back/forth drift range
    public float driftFrequency = 0.6f;

    private Vector3[] offsets;
    private Vector3 currentOffset;
    private float switchTimer;
    private float timeOffset; // For unique motion per drone

    void Start()
    {
        // Find player if not set
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                Debug.Log($"[{name}] Found player automatically: {player.name}");
            }
            else
            {
                Debug.LogWarning($"[{name}] No player found with tag 'Player'. Boss drone will idle.");
            }
        }

        offsets = new Vector3[] { frontOffset, leftOffset, rightOffset };
        PickNewOffset();

        // Offset for motion randomness so multiple drones donÅft move the same
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (player == null) return;

        // Switch position every interval
        switchTimer -= Time.deltaTime;
        if (switchTimer <= 0f)
        {
            PickNewOffset();
        }

        // Base target relative to player
        Vector3 targetPos = player.position
                          + player.forward * currentOffset.z
                          + player.right * currentOffset.x
                          + Vector3.up * hoverHeight;

        // Add natural hover bob
        float bob = Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;

        // Add sideways sway (local X)
        float sway = Mathf.Sin((Time.time + timeOffset) * swayFrequency) * swayAmplitude;

        // Add small drift in Z
        float drift = Mathf.Sin((Time.time + timeOffset) * driftFrequency) * driftAmplitude;

        // Apply motion offsets in local space
        Vector3 extraOffset = player.right * sway + player.forward * drift + Vector3.up * bob;

        Vector3 finalTargetPos = targetPos + extraOffset;

        // Smoothly move
        transform.position = Vector3.Lerp(transform.position, finalTargetPos, Time.deltaTime * moveSpeed);

        // Smoothly look at player
        Vector3 lookDir = player.position - transform.position;
        if (lookDir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * moveSpeed);
        }
    }

    void PickNewOffset()
    {
        currentOffset = offsets[Random.Range(0, offsets.Length)];
        switchTimer = switchInterval + Random.Range(-1f, 1f);
    }
}
