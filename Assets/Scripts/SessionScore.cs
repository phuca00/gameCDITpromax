using UnityEngine;

public class SessionScore : MonoBehaviour
{
    public static int totalScore = 0;

    // Reset khi mở game
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetScore()
    {
        totalScore = 0;
    }
}