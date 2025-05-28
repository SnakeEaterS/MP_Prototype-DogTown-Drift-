using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Splines;
public class Drone : MonoBehaviour
{
    public float shootInterval = 2f;
    public float catchUpSpeed = 3f;
    public float originalSpeed;
    public float targetOffsetT = 0.01f;
    public float slowDownDistance = 3f;
    public Vector2 xyMoveAmplitude = new Vector2(0.5f, 0.5f);
    public Vector2 xyMoveFrequency = new Vector2(1f, 1.5f);
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public static float GlobalTargetT = -1f;
    public static Drone leaderDrone;

    private float shootTimer;
    private BikeSplineFollower follower;
    private Transform player;
    private float targetT;
    private bool reachedTarget = false;
    private Vector3 baseOffset;
    private bool isLeader = false;

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

        // Reflection: get player's t value
        var tField = typeof(BikeSplineFollower).GetField("t", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        BikeSplineFollower playerFollower = player.GetComponent<BikeSplineFollower>();

        if (playerFollower != null && tField != null)
        {
            float playerT = (float)tField.GetValue(playerFollower);
            float spawnT = Mathf.Max(0f, playerT - 0.0075f);
            tField.SetValue(follower, spawnT);

            // LEADER SETUP
            if (leaderDrone == null)
            {
                isLeader = true;
                leaderDrone = this;

                GlobalTargetT = Mathf.Min(1f, playerT + targetOffsetT);
                targetT = GlobalTargetT;
            }
            else
            {
                // FOLLOWER: target the leader's current t
                isLeader = false;
                var leaderField = typeof(BikeSplineFollower).GetField("t", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                float leaderT = (float)leaderField.GetValue(leaderDrone.follower);
                GlobalTargetT = leaderT;
                targetT = GlobalTargetT;
                follower.speed += catchUpSpeed;
            }

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
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (transform.position.z > player.transform.position.z + 0.01)
                {
                    follower.speed = originalSpeed;
                    reachedTarget = true;
                    baseOffset = transform.localPosition;
                }

                // Keep GlobalTargetT synced to leader’s current t
                GlobalTargetT = currentT;
            }
            else
            {
                // Always follow the leader’s t
                if (leaderDrone != null)
                {
                    float leaderT = (float)tField.GetValue(leaderDrone.follower);
                    tField.SetValue(follower, leaderT);
                }

                if (currentT >= targetT)
                {
                    follower.speed = originalSpeed;
                    reachedTarget = true;
                    baseOffset = transform.localPosition;
                }
            }
        }

        if (reachedTarget)
        {
            float x = Mathf.Sin(Time.time * xyMoveFrequency.x) * xyMoveAmplitude.x;
            float y = Mathf.Sin(Time.time * xyMoveFrequency.y) * xyMoveAmplitude.y;
            transform.localPosition = baseOffset + new Vector3(x, y, 0f);
        }

        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab && shootPoint)
        {
            Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        }
    }
}
