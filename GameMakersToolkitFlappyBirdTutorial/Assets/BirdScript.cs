using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdScript : MonoBehaviour
{
    [Tooltip("Required 2D physics of the Bird character.")]
    public Rigidbody2D BirdPhysics;

    [Tooltip("Enables tweaking the strength of the wing-flap of the Bird, when pressing Space.")]
    public float flapStrength = 10;

    [Tooltip("The game logic manager.")]
    public LogicManagerScript LogicManager;

    private bool _isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(BirdPhysics != null, "'BirdPhysics' Rigidbody2D not assigned!");
        Debug.Assert(LogicManager != null, "'LogicManager' LogicManagerScript not assigned!");
        Debug.Assert(flapStrength > 0, "'flapStrength' must be greater than 0!");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && _isAlive) 
        {
            BirdPhysics.velocity = Vector3.up * flapStrength;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _isAlive = false;
        LogicManager.TriggerGameOver();
    }
}
