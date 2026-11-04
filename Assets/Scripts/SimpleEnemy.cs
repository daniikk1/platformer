using System.Collections;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    private int direction = -1;

    public int currentHealth = 2;
    public int contactDamage = 1;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            FlipDirection();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
        }
    }

    void FlipDirection()
    {
        direction *= -1;
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            if (anim != null) anim.SetTrigger("Hurt");
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (anim != null) anim.SetTrigger("Die");

        Destroy(gameObject, 1.5f);
    }
}