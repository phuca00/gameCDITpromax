using UnityEngine;
using Unity.Netcode;

public class Fruits : NetworkBehaviour
{
    [SerializeField] private int scoreValue = 10;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (isCollected) return;

        CollectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void CollectServerRpc()
    {
        if (isCollected) return;

        isCollected = true;

        // 🔥 GỬI điểm về client
        AddScoreClientRpc(scoreValue);

        // báo spawn lại
        SpawnItem spawner = FindObjectOfType<SpawnItem>();
        if (spawner != null)
        {
            spawner.ItemCollected();
        }

        // xoá fruit
        GetComponent<NetworkObject>().Despawn(true);
    }

    [ClientRpc]
    void AddScoreClientRpc(int value)
    {
        if (Score.instance != null)
        {
            Score.instance.AddScore(value);
        }
    }
}