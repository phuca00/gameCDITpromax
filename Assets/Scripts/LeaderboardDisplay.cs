using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class LeaderboardDisplay : MonoBehaviour
{
    public Transform entryContainer;       
    public GameObject entryTemplate;       
    public float viewTime = 5f; 

    void Start()
    {
        if (entryTemplate == null || entryContainer == null) return;
        
        entryTemplate.SetActive(false);
        Invoke(nameof(BuildLeaderboard), 0.5f);

        // Server đếm giờ để chuyển màn
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(GoToNextLevel());
        }
    }

    void BuildLeaderboard()
    {
        PlayerNetwork[] allPlayers = FindObjectsOfType<PlayerNetwork>();
        if (allPlayers.Length == 0) return;

        var sorted = allPlayers.OrderByDescending(p => p.playerScore.Value).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            GameObject newEntry = Instantiate(entryTemplate, entryContainer);
            newEntry.SetActive(true);

            RectTransform rect = newEntry.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -60 * i); 

            TMP_Text[] texts = newEntry.GetComponentsInChildren<TMP_Text>();
            foreach (var t in texts)
            {
                string nameLow = t.gameObject.name.ToLower();
                if (nameLow.Contains("pos")) t.text = (i + 1).ToString();
                else if (nameLow.Contains("name")) t.text = sorted[i].gameObject.name.Replace("(Clone)", "");
                else if (nameLow.Contains("score")) t.text = sorted[i].playerScore.Value.ToString();
            }
        }
    }

    IEnumerator GoToNextLevel()
    {
        yield return new WaitForSeconds(viewTime);

        string lastLevel = PlayerPrefs.GetString("LastLevel", "level1");
        int levelNumber = 1;
        int.TryParse(lastLevel.Replace("level", ""), out levelNumber);
        
        int nextLevel = levelNumber + 1;
        string nextSceneName = (nextLevel > 8) ? "SelectLevel" : "level" + nextLevel;

        // BẾ TẤT CẢ SANG MÀN 2 - KHÔNG XÓA AI CẢ ĐỂ TRÁNH XUNG ĐỘT
        NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}