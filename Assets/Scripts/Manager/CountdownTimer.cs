using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class CountdownTimer : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timeText;

    [Header("Time Settings")]
    [SerializeField] private float startTime = 60f;

    [Header("Delay")]
    [SerializeField] private float leaderboardTime = 2f;

    private NetworkVariable<float> currentTime = new NetworkVariable<float>();
    private bool hasEnded = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentTime.Value = startTime;
        }
    }

    void Update()
    {
        // 🔥 SERVER đếm ngược
        if (IsServer && !hasEnded)
        {
            currentTime.Value -= Time.deltaTime;

            if (currentTime.Value <= 0f)
            {
                currentTime.Value = 0f;
                hasEnded = true;

                StartCoroutine(EndGameFlow());
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (timeText == null) return;

        int seconds = Mathf.CeilToInt(currentTime.Value);
        timeText.text = seconds.ToString();
    }

    IEnumerator EndGameFlow()
    {
        Debug.Log("Hết giờ!");

        // 🔥 LẤY ĐIỂM
        int finalScore = 0;
        if (Score.instance != null)
        {
            finalScore = Score.instance.GetScore();
        }

        // 🔥 LẤY SCENE HIỆN TẠI
        string sceneName = SceneManager.GetActiveScene().name;

        // 🔥 LƯU ĐIỂM + LEVEL
        PlayerPrefs.SetInt(sceneName + "_Score", finalScore);
        PlayerPrefs.SetString("LastLevel", sceneName);
        PlayerPrefs.Save();

        Debug.Log("Saved Score: " + finalScore);

        // 🔥 LOAD LEADERBOARD
        NetworkManager.Singleton.SceneManager.LoadScene("Leaderboard", LoadSceneMode.Single);

        yield return null;
    }
}