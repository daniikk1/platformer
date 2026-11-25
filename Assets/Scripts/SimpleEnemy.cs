using System.Collections;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public EnemyData data; // Ссылка на ScriptableObject
    private int currentHealth;
    private int direction = 1;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = true;

    public Transform player;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private float losePlayerTimer = 0f;

    public LayerMask obstacleLayer;
    public Transform eyesTransform;

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
        FlipDirectionManual(direction > 0);
    }

    void Update()
    {
        if (isDead || data == null) return;

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : 999f;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (distanceToPlayer <= data.attackRange)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                if (anim != null) anim.SetBool("isWalking", false);
            }
            return;
        }

        if (CanSeePlayer())
        {
            losePlayerTimer = data.losePlayerDuration;
        }

        if (losePlayerTimer > 0f)
        {
            losePlayerTimer -= Time.deltaTime;

            float targetDir = player.position.x > transform.position.x ? 1f : -1f;
            bool wallAhead = Physics2D.Raycast(transform.position, targetDir > 0 ? Vector2.right : Vector2.left, 0.6f, obstacleLayer);

            if (wallAhead)
            {
                losePlayerTimer = 0f;
            }
            else
            {
                if ((targetDir > 0 && !facingRight) || (targetDir < 0 && facingRight))
                {
                    FlipDirectionManual(targetDir > 0);
                }

                rb.linearVelocity = new Vector2(data.chaseSpeed * direction, rb.linearVelocity.y);
                if (anim != null) anim.SetBool("isWalking", true);
                return;
            }
        }

        rb.linearVelocity = new Vector2(data.moveSpeed * direction, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
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

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("isWalking", false);

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.enemyWindUpClip);

        yield return new WaitForSeconds(data.windUpTime);

        if (isDead) yield break;

        if (anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(data.attackCooldown - data.windUpTime);
        isAttacking = false;
        nextAttackTime = Time.time + data.attackCooldown;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall") && !isAttacking)
        {
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
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.enemyDeathClip);
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
            anim.ResetTrigger("Attack");
            anim.ResetTrigger("Hurt");
            anim.SetTrigger("Die");
        }

        Destroy(gameObject, 1.5f);
    }

    public void DealDamageToPlayer()
    {
        if (isDead || data == null) return;

        if (player != null && Vector2.Distance(transform.position, player.position) <= data.attackRange * 1.4f)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(data.contactDamage);
            }
        }
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.enemyHitClip);
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}