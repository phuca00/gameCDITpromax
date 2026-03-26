using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Time Settings")]
    [SerializeField] private float startTime = 60f;

    private float currentTime;
    private bool isRunning = true;

    private void Start()
    {
        currentTime = startTime;
        UpdateUI();
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            OnTimeEnd();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        timeText.text = seconds.ToString();
    }

    void OnTimeEnd()
    {
        Debug.Log("Hết giờ!");
        // bạn có thể gọi end game, hiển thị bảng xếp hạng ở đây
    }
}