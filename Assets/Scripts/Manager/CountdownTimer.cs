using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Time Settings")]
    [SerializeField] private float startTime = 20f;
    [SerializeField] private float transitionDelay = 1f;

    private float currentTime;
    private bool isRunning = true;
    private bool hasEnded = false; // tránh gọi EndGame nhiều lần

    private void Start()
    {
        currentTime = startTime;
        UpdateUI();
    }

    private void Update()
    {
        if (!isRunning || hasEnded) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            hasEnded = true;

            StartCoroutine(EndGame());
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        timeText.text = seconds.ToString();
    }

    IEnumerator EndGame()
    {
        // 🔥 Lấy điểm từ hệ thống Score
        int finalScore = 0;

        if (Score.instance != null)
        {
            finalScore = Score.instance.GetScore();
        }
        else
        {
            Debug.LogWarning("Score.instance is NULL!");
        }

        // 🔥 Lấy tên scene hiện tại
        string sceneName = SceneManager.GetActiveScene().name;

        // 🔥 Lưu scene vừa chơi
        PlayerPrefs.SetString("LastScene", sceneName);

        // 🔥 Cộng dồn điểm (SESSION - reset khi tắt game)
        SessionScore.totalScore += finalScore;

        Debug.Log("Scene: " + sceneName);
        Debug.Log("Score màn: " + finalScore);
        Debug.Log("Tổng session: " + SessionScore.totalScore);

        yield return new WaitForSeconds(transitionDelay);

        // 🔥 Chuyển sang Leaderboard
        SceneManager.LoadScene("Leaderboard");
    }
}