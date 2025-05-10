using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    private float damage;
    private float lifetime = 3f;
    private Vector2 direction;
    public LayerMask hitMask;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); 
    }

    public void Initialize(Vector2 dir, float dmg)
    {
        direction = dir.normalized; 
        damage = dmg;
    }

    void FixedUpdate()
    {
        if (rb != null)
            rb.velocity = direction * speed; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & hitMask) != 0)
        {
            if (collision.gameObject.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }

}
