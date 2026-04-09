using UnityEngine;
using Unity.Netcode;

public class Fruits : NetworkBehaviour
{
    public int scoreValue = 10;
    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || isCollected) return; 

        // Tự động quét xem vật chạm vào có phải là người chơi không
        PlayerNetwork pn = collision.GetComponent<PlayerNetwork>();
        if (pn != null)
        {
            isCollected = true;
            pn.AddScore(scoreValue); 
            
            if (GetComponent<NetworkObject>().IsSpawned) 
                GetComponent<NetworkObject>().Despawn(); 
        }
    }
}