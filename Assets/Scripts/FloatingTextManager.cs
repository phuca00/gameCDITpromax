using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    [SerializeField] private GameObject floatingTextPrefab;

    private void Awake()
    {
        // ✅ Singleton + không bị destroy khi đổi scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show(int value, Vector3 worldPosition)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextPrefab chưa gán!");
            return;
        }

        GameObject obj = Instantiate(floatingTextPrefab, worldPosition, Quaternion.identity);

        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.SetText(value);
        }
    }
}