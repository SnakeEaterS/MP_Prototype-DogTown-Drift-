using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
public class Biker : MonoBehaviour
{
    public float spawnT = 0f;
    public float speedBoost = 1f;
    public Vector2 distanceThresholdRange = new Vector2(0f, 10f);
    public float followDistance = 5f; // Minimum distance behind another biker

    private BikeSplineFollower follower;
    private Transform playerTransform;
    private float originalSpeed;
    private float distanceThreshold;
    private bool speedReset = false;
    private static List<Biker> allBikers = new List<Biker>();

    void Start()
    {
        follower = GetComponent<BikeSplineFollower>();
        originalSpeed = follower.speed;
        follower.speed += speedBoost;

        distanceThreshold = Random.Range(distanceThresholdRange.x, distanceThresholdRange.y);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogError("No GameObject with tag 'Player' found.");

        GameObject pathObj = GameObject.FindWithTag("Path");
        if (pathObj != null)
        {
            SplineContainer spline = pathObj.GetComponent<SplineContainer>();
            if (spline != null)
                follower.spline = spline;
            else
                Debug.LogError("SplineContainer component not found on GameObject tagged 'Path'.");
        }
        else
        {
            Debug.LogError("No GameObject found with tag 'Path'.");
        }

        typeof(BikeSplineFollower)
            .GetField("t", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(follower, spawnT);

        follower.currentLane = follower.targetLane = Random.value < 0.5f ? 0 : 2;

        // Add self to list of active bikers
        allBikers.Add(this);
    }

    void OnDestroy()
    {
        allBikers.Remove(this);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Check for player distance and reset speed
        if (!speedReset)
        {
            float distanceToPlayer = transform.position.z - playerTransform.position.z;
            if (distanceToPlayer > distanceThreshold)
            {
                follower.speed = originalSpeed;
                speedReset = true;
            }
        }

        // Check if another biker is ahead in the same lane
        foreach (Biker other in allBikers)
        {
            if (other == this) continue;

            if (other.follower.currentLane == this.follower.currentLane)
            {
                float dz = other.transform.position.z - transform.position.z;
                if (dz > 0 && dz < followDistance)
                {
                    follower.speed = 0f;
                    return;
                }
            }
        }

        follower.speed = speedReset ? originalSpeed : originalSpeed + speedBoost;

    }
}