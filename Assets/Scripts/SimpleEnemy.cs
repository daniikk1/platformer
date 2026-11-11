using System.Collections;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chaseSpeed = 3.5f;
    private int direction = -1;

    public int currentHealth = 2;
    public int contactDamage = 1;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = false;

    public Transform player;
    public float visionRange = 4f;      
    public float attackRange = 1.2f;    
    public float attackCooldown = 1.5f; 
    public float windUpTime = 0.4f; 
    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private float losePlayerTimer = 0f;
    public float losePlayerDuration = 1.5f;

    public LayerMask obstacleLayer; 
    public Transform eyesTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : 999f;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (distanceToPlayer <= attackRange)
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
            losePlayerTimer = losePlayerDuration;
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

                rb.linearVelocity = new Vector2(chaseSpeed * direction, rb.linearVelocity.y);
                if (anim != null) anim.SetBool("isWalking", true);
                return;
            }
        }

        rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > visionRange) return false;

        Vector2 startPos = eyesTransform != null ? (Vector2)eyesTransform.position : (Vector2)transform.position;
        Vector2 directionToPlayer = ((Vector2)player.position - startPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(startPos, directionToPlayer, visionRange, obstacleLayer);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
            return false;
        }

        return true;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("isWalking", false);

        yield return new WaitForSeconds(windUpTime);

        if (isDead) yield break;

        if (anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(attackCooldown - windUpTime);
        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
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

        StopAllCoroutines();

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
        if (isDead) return;

        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange * 1.4f)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(contactDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}