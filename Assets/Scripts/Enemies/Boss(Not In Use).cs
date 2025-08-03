using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public Transform player;
    public float followDistance = 20f;
    public float height = 1410f;
    public float moveSpeed = 10f;
    public float xOffsetRange = 10f; // max offset on the player's right/left side

    private float targetXOffset;  // local offset on player's right vector
    private Vector3 targetPos;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            SetNewTargetOffset();
            UpdateTargetPosition();
        }
    }

    void Update()
    {
        if (player == null) return;

        // Update target position based on current offset and player position/direction
        UpdateTargetPosition();

        // Move towards the target position smoothly
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Check if close enough to target to pick a new offset
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                             new Vector3(targetPos.x, 0, targetPos.z)) < 0.1f)
        {
            SetNewTargetOffset();
        }

        // Keep fixed height
        Vector3 fixedHeightPos = transform.position;
        fixedHeightPos.y = height;
        transform.position = fixedHeightPos;

        // Look at the player
        transform.LookAt(player.position);
    }

    void SetNewTargetOffset()
    {
        // Random offset to the right or left of player
        targetXOffset = Random.Range(-xOffsetRange, xOffsetRange);
    }

    void UpdateTargetPosition()
    {
        // Calculate target position relative to player forward and right vectors
        Vector3 forwardOffset = player.forward * followDistance;
        Vector3 rightOffset = player.right * targetXOffset;

        targetPos = player.position + forwardOffset + rightOffset;
        targetPos.y = height;  // keep constant height
    }
}
