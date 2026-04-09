using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Score : MonoBehaviour
{
    public TMP_Text myScoreText; 

    void Update()
    {
        if (myScoreText == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient) return;

        // Tìm nhân vật của chính mình (Local Player)
        PlayerNetwork myPlayer = null;
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>();
        }
        else
        {
            // Quét dự phòng nếu LocalClient chưa cập nhật kịp
            PlayerNetwork[] allPlayers = FindObjectsOfType<PlayerNetwork>();
            foreach (var p in allPlayers)
            {
                if (p.IsOwner) myPlayer = p;
            }
        }

        // Hiện điểm lên
        if (myPlayer != null)
        {
            myScoreText.text = "Score: " + myPlayer.playerScore.Value;
        }
    }
}