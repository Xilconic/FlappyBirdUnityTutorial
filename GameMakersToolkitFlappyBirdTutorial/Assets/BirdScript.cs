using Assets;
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

    [Tooltip("Determines how deep the Bird character can go before it automatically dies.")]
    public float KillFloorDepth = -6.5f;

    [Tooltip("Determines how high the Bird character can go before it cannot be controlled.")]
    public float ControlCeiling = 6.5f;

    private AudioSource _audioSource;
    [Tooltip("The sound effect clip to be played on death.")]
    public AudioClip DeathSoundEffect;
    [Tooltip("The sound effect clip to be played when flapping.")]
    public AudioClip FlapSoundEffect;

    [Tooltip("The hitspark effect played when player hits an obstacle")]
    public GameObject HitSpark;

    public bool IsAlive {  get; private set; } = true;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(BirdPhysics != null, "'BirdPhysics' Rigidbody2D not assigned!");
        Debug.Assert(LogicManager != null, "'LogicManager' LogicManagerScript not assigned!");
        Debug.Assert(DeathSoundEffect != null, "'DeathSoundEffect' AudioClip not assigned!");
        Debug.Assert(FlapSoundEffect != null, "'FlapSoundEffect' AudioClip not assigned!");
        Debug.Assert(flapStrength > 0, "'flapStrength' must be greater than 0!");
        _audioSource = GetComponent<AudioSource>();
        Debug.Assert(_audioSource != null, "'AudioSource' cannot be found as component!");
        Debug.Assert(HitSpark != null, "'HitSpark' GameObject not assigned.");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && IsAlive && transform.position.y <= ControlCeiling) 
        {
            _audioSource.PlayClip(FlapSoundEffect);
            BirdPhysics.velocity = Vector3.up * flapStrength;
        }

        if(transform.position.y < KillFloorDepth)
        {
            BecomeDead();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BecomeDead();
    }

    private void BecomeDead()
    {
        if(IsAlive)
        {
            IsAlive = false;

            Instantiate(HitSpark, transform); // Not particularly great looking :(

            _audioSource.PlayClip(DeathSoundEffect);
            
            LogicManager.TriggerGameOver();
        }
    }
}
