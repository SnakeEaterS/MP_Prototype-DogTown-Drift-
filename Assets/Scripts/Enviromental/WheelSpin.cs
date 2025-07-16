using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSpin : MonoBehaviour
{
    public GameObject[] wheels;
    public float spinSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject wheel in wheels)
        {
            if (wheel != null)
            {
                // Spin the wheel
                wheel.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            }
        }
    }
}
