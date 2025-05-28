using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSpeedDebugger : MonoBehaviour
{
    void Update()
    {
        var follower = GetComponent<BikeSplineFollower>();
        if (follower != null)
        {
            Debug.Log($"Drone speed = {follower.speed}");
        }
    }
}
