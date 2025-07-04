using UnityEngine;
using System.Collections;

public class LoopingMover : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float minSpeed = 1f;
    public float maxSpeed = 5f;

    public float minWaitTime = 0.5f;
    public float maxWaitTime = 2f;

    private void Start()
    {
        transform.position = pointA.position;
        StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            float moveSpeed = Random.Range(minSpeed, maxSpeed);

            while (Vector3.Distance(transform.position, pointB.position) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, pointB.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Snap to exact end position
            transform.position = pointB.position;

            // Wait for random time
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Teleport back to A
            transform.position = pointA.position;

            // Optional: small wait before next run
            waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
}