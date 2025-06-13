using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFollow : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        // Find the main camera
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (cam == null) return;

        // Make the text face the camera
        transform.LookAt(transform.position + cam.forward);
    }
}
