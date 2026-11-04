using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private float direction = 1f;
    public int damage = 1;

    public void SetDirection(float dir)
    {
        direction = dir;
        if (dir < 0)
        {
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        SimpleEnemy enemy = hitInfo.GetComponent<SimpleEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }


        if (hitInfo.CompareTag("Wall") || hitInfo.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}