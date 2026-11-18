using System.Collections;
using UnityEngine;

public class SkeletonArcher : MonoBehaviour
{
    public EnemyData data;
    private int currentHealth;
    private int direction = -1;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = false;

    public Transform player;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    public GameObject arrowPrefab;
    public Transform shootPoint;

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

        facingRight = true;
        direction = 1;
    }

    void Update()
    {
        if (isDead || data == null) return;

        float horizontalDist = player != null ? Mathf.Abs(transform.position.x - player.position.x) : 999f;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (horizontalDist <= data.attackRange && CanSeePlayer())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("isWalking", false);

            float targetDir = player.position.x > transform.position.x ? 1f : -1f;
            if ((targetDir > 0 && !facingRight) || (targetDir < 0 && facingRight))
            {
                FlipDirectionManual(targetDir > 0);
            }

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(ShootRoutine());
            }
            return;
        }

        rb.linearVelocity = new Vector2(data.moveSpeed * direction, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
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

    IEnumerator ShootRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("isWalking", false);

        yield return new WaitForSeconds(data.windUpTime);

        if (isDead) yield break;

        if (anim != null) anim.SetTrigger("Shoot");

        ShootTwoArrows();

        yield return new WaitForSeconds(data.attackCooldown - data.windUpTime);
        isAttacking = false;
        nextAttackTime = Time.time + data.attackCooldown;
    }

    void ShootTwoArrows()
    {
        if (arrowPrefab == null || shootPoint == null) return;

        float shootDir = facingRight ? 1f : -1f;

        Vector3 pos1 = shootPoint.position;
        GameObject arrow1 = Instantiate(arrowPrefab, pos1, shootPoint.rotation);
        SetupArrow(arrow1, shootDir);

        Vector3 pos2 = shootPoint.position + new Vector3(0, 0.35f, 0);
        GameObject arrow2 = Instantiate(arrowPrefab, pos2, shootPoint.rotation);
        SetupArrow(arrow2, shootDir);
    }

    void SetupArrow(GameObject arrowObj, float dir)
    {
        Arrow arrowScript = arrowObj.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.SetDirection(dir);
        }
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
            anim.ResetTrigger("Shoot");
            anim.ResetTrigger("Hurt");
            anim.SetTrigger("Die");
        }

        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.visionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}