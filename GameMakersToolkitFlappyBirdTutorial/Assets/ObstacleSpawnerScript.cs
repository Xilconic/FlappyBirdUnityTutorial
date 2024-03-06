using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Tooltip("The obstacle to be spawned.")]
    public GameObject Obstacle;

    [Tooltip("Defines after how much time a new obstacle is spawned.")]
    public float SpawnCooldownInSeconds = 2;

    [Tooltip("The level of vertical randimization between spawns.")]
    public float VerticalRandomizationOffset = 2;

    private float _cooldownTimerInSeconds;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(Obstacle != null, "'Obstacle' is not set to a GameObject!");
        Debug.Assert(SpawnCooldownInSeconds > 0, "'SpawnCooldownInSeconds' must be greater than 0!");
        Debug.Assert(VerticalRandomizationOffset >= 0, "'VerticalRandomizationOffset' must be equal to or greater than 0!");
    }

    // Update is called once per frame
    void Update()
    {
        if(_cooldownTimerInSeconds <= 0)
        {
            var lowestPoint = transform.position.y - VerticalRandomizationOffset;
            var highestPoint = transform.position.y + VerticalRandomizationOffset;
            var spawnPosition = new Vector3(transform.position.x, Random.Range(lowestPoint, highestPoint));
            Instantiate(Obstacle, spawnPosition, transform.rotation);
            _cooldownTimerInSeconds = SpawnCooldownInSeconds;
        }
        else
        {
            _cooldownTimerInSeconds -= Time.deltaTime;
        }
    }
}
