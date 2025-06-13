using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetTemporary : MonoBehaviour
{
    public Transform target; // The object to follow (enemy)
    public float duration = 2f; // How long to follow before stopping

    private float timer = 0f;

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position;
        }

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject); // Or stop following, or play end animation
        }
    }
}
