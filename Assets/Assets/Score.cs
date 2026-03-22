using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static Score instance;

    public TMP_Text myScoreText;

    private int scoreNum;

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        scoreNum = 0;
        UpdateScore();
    }

    // Hàm cộng điểm
    public void AddScore(int value)
    {
        scoreNum += value;
        UpdateScore();
    }

    // Update UI
    void UpdateScore()
    {
        myScoreText.text = "Score: " + scoreNum;
    }
}