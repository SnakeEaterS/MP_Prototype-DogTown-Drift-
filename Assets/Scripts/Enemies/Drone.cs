using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public Vector2 xRange = new Vector2(-3f, 3f);  // Random X offset range
    public Vector2 yRange = new Vector2(1f, 4f);   // Random Y offset range
    public float zOffset = -5f;                    // Offset behind player
    public float entryDistance = 20f;              // Distance to spawn from side
    public float moveSpeed = 10f;                  // Speed of entry

    private Transform player;
    private Vector3 localOffset;      // Local offset relative to the player
    private bool isMovingIn = true;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player not found.");
            return;
        }

        player = playerObj.transform;
        transform.SetParent(player);

        // Random local offset to follow
        float randX = Random.Range(xRange.x, xRange.y);
        float randY = Random.Range(yRange.x, yRange.y);
        localOffset = new Vector3(randX, randY, zOffset);

        // Calculate entry spawn position relative to player’s transform
        Vector3 entryLocalOffset = localOffset + new Vector3(entryDistance, 0, 0);
        transform.position = player.TransformPoint(entryLocalOffset);
    }

    void Update()
    {
        if (player == null) return;

        Vector3 targetWorldPos = player.TransformPoint(localOffset);

        if (isMovingIn)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetWorldPos) < 0.1f)
            {
                isMovingIn = false;
            }
        }
        else
        {
            Vector3 adjustedTargetPos = targetWorldPos;

            foreach (Drone otherDrone in FindObjectsOfType<Drone>())
            {
                if (otherDrone == this) continue;

                float dist = Vector3.Distance(adjustedTargetPos, otherDrone.transform.position);
                if (dist < 1f) // Minimum allowed separation
                {
                    // Offset sideways to avoid overlap
                    Vector3 offset = (adjustedTargetPos - otherDrone.transform.position).normalized;
                    offset.y = 0f; // Keep vertical level the same
                    adjustedTargetPos += offset * (1f - dist);
                }
            }

            transform.position = adjustedTargetPos;
        }

        // Optional: lock or match rotation
        transform.rotation = Quaternion.identity; // Or align to player.forward if you want
    }
}