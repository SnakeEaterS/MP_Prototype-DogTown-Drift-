using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public Transform player;
    public float followDistance = 20f;
    public float height = 1410f;
    public float moveSpeed = 10f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;
        Vector3 targetPos = player.position + player.forward * followDistance;
        targetPos.y = height;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        transform.LookAt(player.position);
    }
}