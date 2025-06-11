using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BikerTarget : MonoBehaviour
{
    private JoyconRevController joyconRevController;
    private BikeSplineFollower bikeSplineFollower;

    private Transform target1, target2, target3, target4;

    // These are local offsets from the parent (customize for your biker lanes)
    public Vector3 offset1 = new Vector3(-1.5f, 0f, -2f);
    public Vector3 offset2 = new Vector3(1.5f, 0f, -2f);
    public Vector3 offset3 = new Vector3(1.5f, 0f, 1f);
    public Vector3 offset4 = new Vector3(-1.5f, 0f, 1f);

    void Start()
    {
        joyconRevController = FindObjectOfType<JoyconRevController>();
        bikeSplineFollower = GetComponent<BikeSplineFollower>();

        if (joyconRevController == null)
            Debug.LogError("JoyconRevController not found in scene.");
        if (bikeSplineFollower == null)
            Debug.LogError("BikeSplineFollower not found on this GameObject.");

        // Create and position targets as children of this object
        target1 = CreateTarget("Target1", offset1);
        target2 = CreateTarget("Target2", offset2);
        target3 = CreateTarget("Target3", offset3);
        target4 = CreateTarget("Target4", offset4);
    }

    void Update()
    {
        if (joyconRevController != null && bikeSplineFollower != null)
        {
            bikeSplineFollower.speed = joyconRevController.speed; // Sync speed
        }
    }

    Transform CreateTarget(string name, Vector3 localOffset)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = localOffset;
        obj.transform.localRotation = Quaternion.identity;
        return obj.transform;
    }
}
