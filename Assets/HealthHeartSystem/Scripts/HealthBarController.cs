using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private GameObject[] heartLists;

    private PlayerHealth _playerHealth;

    public void SetPlayerHealth(PlayerHealth playerHealth)
    {
        this._playerHealth = playerHealth;
    }

    private void Update()
    {
        // ❗ Tránh lỗi null khi chưa gán PlayerHealth
        if (_playerHealth == null) return;

        // Tắt toàn bộ tim
        for (int i = 0; i < heartLists.Length; i++)
        {
            if (heartLists[i] != null)
                heartLists[i].SetActive(false);
        }

        // Bật tim theo máu hiện tại (có chặn vượt mảng)
        int healthToShow = Mathf.Clamp(_playerHealth.currentHealth, 0, heartLists.Length);

        for (int i = 0; i < healthToShow; i++)
        {
            if (heartLists[i] != null)
                heartLists[i].SetActive(true);
        }
    }
}