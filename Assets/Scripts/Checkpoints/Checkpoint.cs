using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public static event System.Action OnCheckpointActivated;

    [SerializeField] private Animator animator;
    [SerializeField] private string animationName = "checkpoint";

    [SerializeField] private float _transitionDelay = 1f;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated) return;

        if (!collision.CompareTag("Player")) return;

        activated = true;

        // 🔥 FIX QUAN TRỌNG: Tắt UI ngay lập tức
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            canvas.SetActive(false);
        }

        AudioManager.instance.PlayCheckpoint();

        // Lưu tiến trình
        PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "_Completed", 1);

        // Chạy animation checkpoint
        if (animator != null)
            animator.Play(animationName);

        // Delay rồi mới chuyển scene
        StartCoroutine(ActivateCheckpointWithDelay(1));
    }

    private IEnumerator ActivateCheckpointWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        OnCheckpointActivated?.Invoke();
    }
}