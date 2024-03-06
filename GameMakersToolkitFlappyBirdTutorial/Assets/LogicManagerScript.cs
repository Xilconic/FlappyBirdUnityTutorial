using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogicManagerScript : MonoBehaviour
{
    [Tooltip("The element displaying the score.")]
    public Text ScoreUI;

    private int _score = 0;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(ScoreUI != null, "'ScoreUI' is not set to a Text!");
    }

    [ContextMenu("Increase Score")]
    public void IncrementScore()
    {
        _score++;
        ScoreUI.text = _score.ToString();
    }
}
