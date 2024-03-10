using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Prevents destroying this object between scene switches, causing it to carry over between levels:
        // This ensures we continuously keep playing the background music.
        DontDestroyOnLoad(gameObject);
    }
}
