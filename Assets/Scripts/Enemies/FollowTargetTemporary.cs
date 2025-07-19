using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetTemporary : MonoBehaviour
{
    public Transform target; // The object to follow (enemy)
    public float duration = 2f; // How long to follow before stopping
    public bool followRotation = false; // Whether to follow the target's rotation
    public Vector3 rotationOffset = Vector3.zero; // Additional rotation offset

    private float timer = 0f;

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position;
        }

        if (followRotation && target != null)
        {
            transform.rotation = target.rotation * Quaternion.Euler(rotationOffset);
        }

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}