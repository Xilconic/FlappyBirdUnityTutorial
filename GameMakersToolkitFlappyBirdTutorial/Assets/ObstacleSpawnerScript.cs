using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Tooltip("The obstacle to be spawned.")]
    public GameObject Obstacle;

    [Tooltip("Defines after how much time a new obstacle is spawned.")]
    public float SpawnCooldownInSeconds = 2;

    private float _cooldownTimerInSeconds;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(Obstacle != null, "'Obstacle' is not set to a GameObject!");
        Debug.Assert(SpawnCooldownInSeconds > 0, "'SpawnCooldownInSeconds' must be greater than 0!");
    }

    // Update is called once per frame
    void Update()
    {
        if(_cooldownTimerInSeconds <= 0)
        {
            Instantiate(Obstacle, transform.position, transform.rotation);
            _cooldownTimerInSeconds = SpawnCooldownInSeconds;
        }
        else
        {
            _cooldownTimerInSeconds -= Time.deltaTime;
        }
    }
}
