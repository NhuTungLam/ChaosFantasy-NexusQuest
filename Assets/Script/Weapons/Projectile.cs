using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage;
    public float lifetime = 3f;
    public Vector2 direction;
    public LayerMask hitMask;
    private Rigidbody2D rb;
    public bool canPierce;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & hitMask) != 0)
        {
            if (collision.gameObject.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damage);
            }
            if (canPierce == false)
            {
                Destroy(gameObject);
            }
        }

    }
    
}
