using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdScript : MonoBehaviour
{
    public Rigidbody2D BirdPhysics;
    public float flapStrength = 10;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(BirdPhysics != null, "'BirdPhysics' Rigidbody2D not assigned!");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            BirdPhysics.velocity = Vector3.up * flapStrength;
        }
    }
}
