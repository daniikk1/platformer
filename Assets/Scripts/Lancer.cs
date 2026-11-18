using System.Collections;
using UnityEngine;

public class Lancer : MonoBehaviour
{
    public EnemyData data;
    private int currentHealth;
    private int direction = -1;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = false;

    public Transform player;
    public Transform eyesTransform;
    public LayerMask obstacleLayer;

    private bool isCharging = false;
    private bool isPreparingCharge = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (data != null) currentHealth = data.maxHealth;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        facingRight = true;
        direction = 1;
    }

    void Update()
    {
        if (isDead || data == null) return;

        if (isCharging)
        {
            rb.linearVelocity = new Vector2(data.chargeSpeed * direction, rb.linearVelocity.y);
            CheckPlayerHitDuringCharge();
            return;
        }

        if (isPreparingCharge)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (CanSeePlayer())
        {
            float targetDir = player.position.x > transform.position.x ? 1f : -1f;
            if ((targetDir > 0 && !facingRight) || (targetDir < 0 && facingRight))
            {
                FlipDirectionManual(targetDir > 0);
            }

            StartCoroutine(ChargeRoutine());
            return;
        }

        rb.linearVelocity = new Vector2(data.moveSpeed * direction, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
    }

    void CheckPlayerHitDuringCharge()
    {
        if (player == null || data == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= 1.2f)
        {
            PlayerMovement playerMov = player.GetComponent<PlayerMovement>();
            if (playerMov != null)
            {
                playerMov.TakeDamage(data.contactDamage);

                isCharging = false;
                FlipDirection();
            }
        }
    }

    bool CanSeePlayer()
    {
        if (player == null || data == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > data.visionRange) return false;

        Vector2 startPos = eyesTransform != null ? (Vector2)eyesTransform.position : (Vector2)transform.position;
        Vector2 directionToPlayer = ((Vector2)player.position - startPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(startPos, directionToPlayer, data.visionRange, obstacleLayer);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player")) return true;
            return false;
        }
        return true;
    }

    IEnumerator ChargeRoutine()
    {
        isPreparingCharge = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("isWalking", false);

        if (anim != null) anim.SetTrigger("PrepareCharge");

        yield return new WaitForSeconds(data.windUpTime);

        if (isDead) yield break;

        isPreparingCharge = false;
        isCharging = true;
        if (anim != null) anim.SetTrigger("Charge");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (isCharging)
            {
                isCharging = false;
            }
            FlipDirection();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (isCharging)
            {
                isCharging = false;
            }
            FlipDirection();
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

    void FlipDirectionManual(bool faceRight)
    {
        if (facingRight == faceRight) return;
        facingRight = faceRight;
        direction = faceRight ? 1 : -1;
        Vector3 scaler = transform.localScale;
        scaler.x = faceRight ? Mathf.Abs(scaler.x) : -Mathf.Abs(scaler.x);
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
        StopAllCoroutines();

        LevelManager manager = Object.FindAnyObjectByType<LevelManager>();
        if (manager != null)
        {
            manager.EnemyDied();
        }

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (anim != null)
        {
            anim.ResetTrigger("PrepareCharge");
            anim.ResetTrigger("Charge");
            anim.SetTrigger("Die");
        }

        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, data.visionRange);
    }
}