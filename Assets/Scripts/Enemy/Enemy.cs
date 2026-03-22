using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float stompBounceForce = 10f;
    public float enemyDamageKnockback = 6f;

    public Animator animator;

    private bool isDead = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.collider.CompareTag("Player")) return;

        PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();
        Rigidbody2D playerRb = collision.collider.GetComponent<Rigidbody2D>();

        ContactPoint2D contact = collision.contacts[0];

        // Player đạp đầu
        if (contact.normal.y < -0.5f)
        {
            StartCoroutine(DieCoroutine());     // CHẠY ANIMATION RỒI MỚI TẮT OBJECT

            playerRb.velocity = new Vector2(playerRb.velocity.x, stompBounceForce);
            return;
        }

        // Enemy húc player
        Vector2 knockbackDir = (playerRb.transform.position - transform.position).normalized;
        Vector2 knockback = knockbackDir * enemyDamageKnockback;

        player.TakeDamage(1, knockback);
    }

    IEnumerator DieCoroutine()
    {
        isDead = true;

        animator.Play("deathend");

        // Lấy length của animation deathend
        float animTime = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animTime);

        gameObject.SetActive(false);   // hoặc Destroy(gameObject);
    }
}