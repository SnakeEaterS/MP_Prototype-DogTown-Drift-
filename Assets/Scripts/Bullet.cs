using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10;

    void Start()
    {
        Destroy(gameObject, 3f); // Destroy after 3 seconds
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
