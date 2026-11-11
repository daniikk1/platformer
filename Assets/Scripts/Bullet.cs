using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    private float direction = 1f;

    public void SetDirection(float dir)
    {
        direction = dir;
        if (direction < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.gameObject);
    }

    void CheckHit(GameObject hitObject)
    {
        if (hitObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            SimpleEnemy orc = hitObject.GetComponent<SimpleEnemy>() ?? hitObject.GetComponentInParent<SimpleEnemy>();
            if (orc != null)
            {
                orc.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            SkeletonArcher skeleton = hitObject.GetComponent<SkeletonArcher>() ?? hitObject.GetComponentInParent<SkeletonArcher>();
            if (skeleton != null)
            {
                skeleton.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            Lancer lancer = hitObject.GetComponent<Lancer>() ?? hitObject.GetComponentInParent<Lancer>();
            if (lancer != null)
            {
                lancer.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        if (hitObject.CompareTag("Wall") ||
            hitObject.CompareTag("Ground") ||
            hitObject.layer == LayerMask.NameToLayer("Ground") ||
            hitObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}