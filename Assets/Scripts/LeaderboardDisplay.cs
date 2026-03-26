using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float waitTime = 3f;

    private void Start()
    {
        string lastScene = PlayerPrefs.GetString("LastScene", "level1");
        int score = PlayerPrefs.GetInt(lastScene + "_Score", 0);

        scoreText.text = SessionScore.totalScore.ToString();

        StartCoroutine(LoadNextLevel(lastScene));
    }

    IEnumerator LoadNextLevel(string lastScene)
    {
        yield return new WaitForSeconds(waitTime);

        int levelNumber = int.Parse(lastScene.Replace("level", ""));
        int nextLevel = levelNumber + 1;

        if (nextLevel > 8)
        {
            // đã xong level8 → quay về màn chọn level
            SceneManager.LoadScene("selectlevel");
        }
        else
        {
            SceneManager.LoadScene("level" + nextLevel);
        }
    }
}