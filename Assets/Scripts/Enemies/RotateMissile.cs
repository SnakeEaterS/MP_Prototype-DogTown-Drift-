using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMissile : MonoBehaviour
{
    public float rotationSpeed = 360f;

    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
