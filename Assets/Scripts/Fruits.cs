using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Phát âm thanh
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayFruit();
            }

            // Cộng điểm
            if (Score.instance != null)
            {
                Score.instance.AddScore(1);
            }

            // Ẩn fruit
            gameObject.SetActive(false);
        }
    }
}