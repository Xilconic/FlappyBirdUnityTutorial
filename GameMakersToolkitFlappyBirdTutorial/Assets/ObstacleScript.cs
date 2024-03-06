using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    /// <summary>
    /// Obstacle destroys itself after reaching this point.
    /// </summary>
    private const float deadZoneX = -13;

    [Tooltip("Defines the speed this obstacle moves from right to left on the screen.")]
    public float moveSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x <= deadZoneX)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.position = transform.position + (Vector3.left * moveSpeed) * Time.deltaTime;
        }
    }
}
