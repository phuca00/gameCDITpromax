using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnItem : MonoBehaviour
{
    [Header("Danh sách prefab có thể spawn")]
    public GameObject[] itemPrefabs;

    [Header("Thời gian spawn lại")]
    public float spawnDelay = 2f;

    private List<Transform> spawnPoints = new List<Transform>();
    private NetworkObject currentItem;

    void Awake()
    {
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }

    void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Không có NetworkManager!");
            return;
        }

        // 🔥 CHỈ SERVER spawn
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server bắt đầu spawn fruit");
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (currentItem == null)
            {
                SpawnRandomItem();
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnRandomItem()
    {
        if (spawnPoints.Count == 0 || itemPrefabs.Length == 0)
        {
            Debug.LogError("Thiếu spawnPoints hoặc itemPrefabs!");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        if (prefab == null)
        {
            Debug.LogError("Prefab NULL!");
            return;
        }

        GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("Prefab thiếu NetworkObject!");
            return;
        }

        netObj.Spawn();

        currentItem = netObj;

        Debug.Log("Spawn fruit: " + prefab.name);
    }

    public void ItemCollected()
    {
        currentItem = null;
    }
}