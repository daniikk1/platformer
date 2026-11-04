using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    private float keyboardInput;
    private float mobileInput;
    private bool facingRight = true;

    public float invulnerableDuration = 2f;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;

    public float jumpForce = 12f;
    private bool isGrounded;
    public UnityEngine.Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    public UnityEngine.Transform firePoint;
    public GameObject bulletPrefab;
    public float attackCooldown = 0.8f;
    private float nextAttackTime = 0f;

    public int maxHealth = 3;
    public int currentHealth;
    public TMPro.TextMeshProUGUI healthText;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateHealthUI();
    }


    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth > 0)
        {
            if (anim != null) anim.SetTrigger("Hurt");

            StartCoroutine(InvulnerabilityRoutine());
        }
        else
        {
            Die();
        }
    }

    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsedTime = 0f;
        while (elapsedTime < invulnerableDuration)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = (color.a == 1f) ? 0.4f : 1f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(0.15f);
            elapsedTime += 0.15f;
        }

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        isInvulnerable = false;
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "" + currentHealth;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }


        if (anim != null) anim.SetTrigger("Die");

        StartCoroutine(RestartLevelAfterDelay(2f));
    }

    IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void Update()
    {
        // Движение влево-вправо
        keyboardInput = Input.GetAxisRaw("Horizontal");
        float moveInput = keyboardInput != 0 ? keyboardInput : mobileInput;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Поворот спрайта
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();

        // Проверка земли (лучем или кругом)
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // Прыжок по нажатию Пробела
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Передача параметров ходьбы в аниматор
        anim.SetBool("isWalking", moveInput != 0);
    }


    public void StartMoving(float direction)
    {
        mobileInput = direction;
    }

    public void StopMoving()
    {
        mobileInput = 0f;
    }

    public void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; // Принудительно гасим флаг, чтобы повторный клик в том же прыжке не сработал
        }
    }

    public void TryAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        anim.SetTrigger("Attack");
        StartCoroutine(ShootWithDelay(0.6f));
    }

    IEnumerator ShootWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bulletPrefab && firePoint)
        {
            GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                float direction = facingRight ? 1f : -1f;
                bulletScript.SetDirection(direction);
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}