using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;

    [Header("Effect")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lifeTime = 1.2f;

    private Color startColor;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshPro>();
    }

    public void SetText(int value)
    {
        text.text = "+" + value;

        // 🎨 màu theo điểm
        if (value >= 50)
            text.color = Color.yellow;
        else if (value >= 20)
            text.color = Color.green;
        else
            text.color = Color.white;

        startColor = text.color;
    }

    private void Update()
    {
        // bay lên
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // fade out
        float t = lifeTime;
        lifeTime -= Time.deltaTime;

        if (text != null)
        {
            Color c = startColor;
            c.a = lifeTime / t;
            text.color = c;
        }

        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}