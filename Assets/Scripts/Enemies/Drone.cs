using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Splines;

public class Drone : MonoBehaviour
{
    public float spawnT;
    public float shootInterval = 2f;
    public float catchUpSpeed = 3f;
    public float originalSpeed;
    public float targetOffsetT = 0.01f;
    public bool isLeader = false;
    public float slowDownDistance = 3f;
    public Vector2 xyMoveAmplitude = new Vector2(0.5f, 0.5f);
    public Vector2 xyMoveFrequency = new Vector2(1f, 1.5f);
    public float attachDistanceThreshold = 0.3f;

    public GameObject bulletPrefab;
    public Transform shootPoint;
    public static Drone LeaderDrone;
    public static float GlobalTargetT = -1f;

    private float shootTimer;
    private BikeSplineFollower follower;
    private Transform player;
    private float targetT;
    private bool reachedTarget = false;
    private bool isAttachedToPlayer = false;
    private Vector3 baseOffset;
    private Vector3 targetPosition;

    void Start()
    {
        follower = GetComponent<BikeSplineFollower>();
        if (follower == null)
        {
            Debug.LogError("BikeSplineFollower not found on Drone.");
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player not found.");
            return;
        }

        player = playerObj.transform;

        GameObject pathObj = GameObject.FindGameObjectWithTag("Path");
        SplineContainer spline = pathObj?.GetComponent<SplineContainer>();
        if (spline == null)
        {
            Debug.LogError("SplineContainer not found on object with tag 'Path'.");
            return;
        }

        originalSpeed = follower.speed;
        follower.spline = spline;

        var tField = typeof(BikeSplineFollower).GetField("t", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        BikeSplineFollower playerFollower = player.GetComponent<BikeSplineFollower>();

        if (playerFollower != null && tField != null)
        {
            float playerT = (float)tField.GetValue(playerFollower);
            float spawnT = Mathf.Max(0f, playerT - 0.0075f);
            tField.SetValue(follower, spawnT);

            if (LeaderDrone == null)
            {
                isLeader = true;
                LeaderDrone = this;
                targetT = Mathf.Min(1f, playerT + targetOffsetT);
            }
            else
            {
                isLeader = false;
                targetT = LeaderDrone.GetTargetT();
            }

            follower.speed += catchUpSpeed;

            targetPosition = follower.spline.EvaluatePosition(targetT);
            baseOffset = transform.localPosition;
        }
        else
        {
            Debug.LogError("Player does not have BikeSplineFollower or couldn't get 't'.");
        }
    }

    void Update()
    {
        var tField = typeof(BikeSplineFollower).GetField("t", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float currentT = (float)tField.GetValue(follower);

        if (!reachedTarget)
        {
            if (isLeader)
            {
                var playerT = (float)tField.GetValue(player.GetComponent<BikeSplineFollower>());
                targetT = Mathf.Min(1f, playerT + targetOffsetT);
                targetPosition = follower.spline.EvaluatePosition(targetT);
            }
            else if (LeaderDrone != null)
            {
                targetT = LeaderDrone.GetTargetT();
                targetPosition = LeaderDrone.transform.localPosition;
            }

            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distanceToTarget < 0.5f)
            {
                Debug.Log("reached");
                follower.speed = originalSpeed;
                reachedTarget = true;
                baseOffset = transform.localPosition;
            }
        }

        if (reachedTarget && !isAttachedToPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < attachDistanceThreshold)
            {
                transform.SetParent(player);
                isAttachedToPlayer = true;
                transform.localPosition = baseOffset;
            }
        }

        // Wavy motion when hovering
        if (reachedTarget && !isAttachedToPlayer)
        {
            float x = Mathf.Sin(Time.time * xyMoveFrequency.x) * xyMoveAmplitude.x;
            float y = Mathf.Sin(Time.time * xyMoveFrequency.y) * xyMoveAmplitude.y;
            transform.localPosition = baseOffset + new Vector3(x, y, 0f);
        }

        // Shooting
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    public float GetTargetT()
    {
        return targetT;
    }

    void Shoot()
    {
        if (bulletPrefab && shootPoint)
        {
            Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        }
    }
}
