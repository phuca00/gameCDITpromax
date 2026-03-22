using System.Collections;         // BẮT BUỘC ĐỂ DÙNG IEnumerator
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public Animator animator;
    public Rigidbody2D rb;

    public float invincibleTime = 0.3f;
    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dmg, Vector2 knockback)
    {
        if (isInvincible) return;

        currentHealth -= dmg;

        if (currentHealth > 0)
        {
            // Animation bị đánh
            AudioManager.instance.PlayDamage();
            animator.SetTrigger("death");

            // Knockback
            rb.velocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);

            StartCoroutine(Invincible());
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        animator.Play("deathend");

        rb.velocity = Vector2.zero;
        GetComponent<PlayerMovement>().enabled = false;
        AudioManager.instance.PlayOver();

        GameManager.instance.PlayerDied();   // báo lên GameManager
    }

    IEnumerator Invincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
}