using UnityEngine;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    public NetworkVariable<int> score = new NetworkVariable<int>(0);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        score.OnValueChanged += OnScoreChanged;

        // cập nhật lần đầu
        UpdateScoreUI(score.Value);
    }

    void OnScoreChanged(int oldValue, int newValue)
    {
        UpdateScoreUI(newValue);
    }

    void UpdateScoreUI(int value)
    {
        if (Score.instance != null)
        {
            Score.instance.AddScore(value - Score.instance.GetScore());
        }
    }

    public void AddScore(int amount)
    {
        if (!IsServer) return;

        score.Value += amount;
    }
}