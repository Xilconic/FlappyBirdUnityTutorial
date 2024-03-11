using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets;

public class LogicManagerScript : MonoBehaviour
{
    [Tooltip("The element displaying the score.")]
    public Text ScoreUI;
    [Tooltip("The element displaying the high score.")]
    public Text HighScoreUI;
    [Tooltip("The game over screen.")]
    public GameObject GameOverScreen;
    [Tooltip("The sound effect to be played when a point is gained.")]
    public AudioClip GainPointSoundEffect;
    [Tooltip("The sound effect to be played when high score is improved.")]
    public AudioClip HighScoreSoundEffect;

    private AudioSource _audioSource;
    /// <remarks>
    /// This field can be null, in case the scene does not have a background music.
    /// </remarks>
    private BackgroundMusicScript _backGroundMusicScript;

    private int _score = 0;
    private SaveData _saveData = new();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(ScoreUI != null, "'ScoreUI' is not set to a Text!");
        Debug.Assert(HighScoreUI != null, "'HighScoreUI' is not set to a Text!");
        Debug.Assert(GameOverScreen != null, "'GameOverScreen' is not set to a GameObject!");
        _audioSource = GetComponent<AudioSource>();
        Debug.Assert(_audioSource != null, "'AudioSource' cannot be found as component!");

        var backGroundMusic = GameObject.FindGameObjectWithTag("BackgroundMusic");
        if(backGroundMusic != null)
        {
            _backGroundMusicScript = backGroundMusic.GetComponent<BackgroundMusicScript>();
            Debug.Assert(_backGroundMusicScript != null, "'BackgroundMusicScript' cannot be found as component of GameObject tagged 'BackgroundMusic'.");
        }

        try
        {
            _saveData = SaveData.Load();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.LogWarning("Due to failing to load save data, will use default SaveData...");
            _saveData = new SaveData();
        }
        UpdateHighScoreUI();
    }

    [ContextMenu("Increase Score")]
    public void IncrementScore()
    {
        if(_saveData.IsBeatingHighScore(_score))
        {
            _audioSource.PlayClip(HighScoreSoundEffect);
        }
        else
        {
            _audioSource.PlayClip(GainPointSoundEffect);
        }

        _score++;
        ScoreUI.UpdateText(_score);
    }

    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        if(_backGroundMusicScript != null)
        {
            _backGroundMusicScript.TargetPitch = 1.0f;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ContextMenu("Trigger game over")]
    public void TriggerGameOver()
    {
        GameOverScreen.SetActive(true);

        if(_backGroundMusicScript != null)
        {
            _backGroundMusicScript.TargetPitch = 0.8f;
        }
        
        if (_saveData.IsBeatingHighScore(_score))
        {
            _saveData.HighScore = _score;
            UpdateHighScoreUI();
            _saveData.Save(); // TODO: I wonder if it's better to save on closing the game instead?
        }
    }

    private void UpdateHighScoreUI()
    {
        HighScoreUI.UpdateText(_saveData.HighScore);
    }
}
