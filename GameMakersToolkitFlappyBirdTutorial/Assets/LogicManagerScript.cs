using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets;

public class LogicManagerScript : MonoBehaviour
{
    [Tooltip("The element displaying the score.")]
    public Text ScoreUI;
    [Tooltip("The game over screen.")]
    public GameObject GameOverScreen;
    [Tooltip("The sound effect to be played when a point is gained.")]
    public AudioClip GainPointSoundEffect;

    private AudioSource _audioSource;

    private int _score = 0;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(ScoreUI != null, "'ScoreUI' is not set to a Text!");
        Debug.Assert(GameOverScreen != null, "'GameOverScreen' is not set to a GameObject!");
        _audioSource = GetComponent<AudioSource>();
        Debug.Assert(_audioSource != null, "'AudioSource' cannot be found as component!");
    }

    [ContextMenu("Increase Score")]
    public void IncrementScore()
    {
        _audioSource.PlayClip(GainPointSoundEffect);

        _score++;
        ScoreUI.text = _score.ToString();
    }

    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ContextMenu("Trigger game over")]
    public void TriggerGameOver()
    {
        GameOverScreen.SetActive(true);
    }
}
