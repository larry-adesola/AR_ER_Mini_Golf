using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class StrokeScore : MonoBehaviour
{
    TMP_Text tmpComponent;
    [SerializeField] int score = 0;
    // Start is called before the first frame update
    void Start()
    {
        tmpComponent = GetComponent<TMP_Text>();
        UpdateScore();
        HideScore();
    }

    //increment score is called from dragshoot
    //reset score is called from placegolfobjects
    public void IncrementScore(int increment = 1)
    {
        score += increment;
        UpdateScore();
    }

    public int getScore()
    {
        return score;
    }
    public void resetScore()
    {
        score = 0;
        UpdateScore();
    }
    public void UpdateScore()
    {
        tmpComponent.text = $"Strokes: {score}";
    }

    public void HideScore()//consider using this instead of setactive
    {
        GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
    }
    public void ShowScore()
    {
        GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
    }
}
