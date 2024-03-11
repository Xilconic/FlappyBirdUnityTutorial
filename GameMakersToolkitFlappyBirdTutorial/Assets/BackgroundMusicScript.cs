using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicScript : MonoBehaviour
{
    [Tooltip("The source of the background music.")]
    public AudioSource Source;
    [Tooltip("Target pitch tracking speed.")]
    public float PitchTrackingSpeed = 1.0f;

    public float TargetPitch { get; set; } = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Prevents destroying this object between scene switches, causing it to carry over between levels:
        // This ensures we continuously keep playing the background music.
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(Source.pitch > TargetPitch)
        {
            Source.pitch -= Mathf.Min(Time.deltaTime * PitchTrackingSpeed, Source.pitch - TargetPitch);
        }
        else if (Source.pitch < TargetPitch)
        {
            Source.pitch += Mathf.Min(Time.deltaTime * PitchTrackingSpeed, TargetPitch - Source.pitch);
        }
    }
}
