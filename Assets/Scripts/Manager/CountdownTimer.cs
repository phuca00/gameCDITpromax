using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class CountdownTimer : NetworkBehaviour
{
    [Header("UI")] [SerializeField] private TMP_Text timeText;

    [Header("Time Settings")] [SerializeField]
    private float startTime = 60f;

    // Biến đồng bộ thời gian từ Server xuống tất cả Client
    private NetworkVariable<float> currentTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private bool hasEnded = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentTime.Value = startTime;
            hasEnded = false;
        }
    }

    void Update()
    {
        // CHỈ SERVER mới được phép đếm thời gian
        if (IsServer && !hasEnded)
        {
            currentTime.Value -= Time.deltaTime;

            if (currentTime.Value <= 0f)
            {
                currentTime.Value = 0f;
                hasEnded = true;

                // Server thực hiện luồng kết thúc game
                StartCoroutine(EndGameFlow());
            }
        }

        // Mọi máy đều tự cập nhật UI của riêng mình
        UpdateUI();
    }

    void UpdateUI()
    {
        if (timeText == null) return;

        // Dùng CeilToInt để hiển thị thời gian thân thiện
        int seconds = Mathf.CeilToInt(currentTime.Value);
        // Tránh việc giây hiển thị số âm trên màn hình Client
        timeText.text = Mathf.Max(0, seconds).ToString();
    }

    IEnumerator EndGameFlow()
    {
        // 1. LƯU LẠI TÊN MÀN CHƠI ĐỂ BIẾT ĐƯỜNG MÀ SANG MÀN TIẾP THEO
        if (IsServer)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("LastLevel", currentScene);
            PlayerPrefs.Save();
        }

        FreezeAndKeepPlayersClientRpc();
        yield return new WaitForSeconds(1.0f); // Đợi mạng ổn định

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Leaderboard", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    void FreezeAndKeepPlayersClientRpc()
    {
        PlayerNetwork[] players = FindObjectsOfType<PlayerNetwork>();
        foreach (var p in players)
        {
            p.transform.SetParent(null);
            DontDestroyOnLoad(p.gameObject); // Phép màu giữ điểm số qua màn mới
            
            if (p.rb != null) {
                p.rb.velocity = Vector2.zero;
                p.rb.simulated = false;
            }
            p.enabled = false;
        }
    }
}