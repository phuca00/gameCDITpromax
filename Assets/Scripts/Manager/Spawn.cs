using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] playerPrefabs;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("✅ SpawnManager started");

        // 1. Spawn cho những thằng đã connect trước đó
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }

        // 2. Nghe thêm client mới
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    void SpawnPlayer(ulong clientId)
    {
        Debug.Log("🔥 Spawn player: " + clientId);

        int index = PlayerPrefs.GetInt("SelectedPlayerIndex", 0);
        if (index >= playerPrefabs.Length) index = 0;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject player = Instantiate(
            playerPrefabs[index],
            spawnPoint.position,
            Quaternion.identity
        );

        player.GetComponent<NetworkObject>()
            .SpawnAsPlayerObject(clientId, true);
    }
}