using UnityEngine;

public class DroneBossFollower : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform player;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float switchInterval = 3f;
    public float hoverHeight = 5f;

    [Header("Offsets")]
    public Vector3 leftOffset = new Vector3(-5f, 0, 3f);   // add Z
    public Vector3 rightOffset = new Vector3(5f, 0, 3f);   // add Z

    [Header("Hover Motion")]
    public float bobAmplitude = 0.5f;
    public float bobFrequency = 1.5f;

    public float swayAmplitude = 0.5f;
    public float swayFrequency = 0.8f;

    public float driftAmplitude = 0.5f;
    public float driftFrequency = 0.6f;

    private Vector3[] offsets;
    private Vector3 currentOffset;
    private float switchTimer;
    private float timeOffset;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        offsets = new Vector3[] { leftOffset, rightOffset };
        PickNewOffset();

        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (player == null) return;

        switchTimer -= Time.deltaTime;
        if (switchTimer <= 0f) PickNewOffset();

        Vector3 targetPos = player.position
                          + player.right * currentOffset.x
                          + player.forward * currentOffset.z //NOW YOU HAVE FORWARD OFFSET!
                          + Vector3.up * hoverHeight;

        float bob = Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;
        float sway = Mathf.Sin((Time.time + timeOffset) * swayFrequency) * swayAmplitude;
        float drift = Mathf.Sin((Time.time + timeOffset) * driftFrequency) * driftAmplitude;

        Vector3 extraOffset = player.right * sway + player.forward * drift + Vector3.up * bob;
        Vector3 finalTargetPos = targetPos + extraOffset;

        transform.position = Vector3.Lerp(transform.position, finalTargetPos, Time.deltaTime * moveSpeed);

        Vector3 lookDir = transform.position - player.position;
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
