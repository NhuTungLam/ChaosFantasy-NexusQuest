using UnityEngine;

public class StationaryRangedEnemy : EnemyMovement
{
    private float attackCooldown;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>(); 
    }

    protected override void Attack()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            return;
        }
        if (player == null || enemyHandler == null)
        {
            Debug.LogError("Player or EnemyHandler is not assigned!");
            return;
        }
        if (Vector2.Distance(player.position, transform.position) <= 7)
        {
            GameObject proj = EnemyProjectileManager.Instance.GetFromPool("enemyProjectile");
            proj.transform.position = transform.position;

            Vector2 dir = (player.position - transform.position).normalized;
            float speed = enemyHandler.enemyData.ProjectileSpeed;

            var hitbox = proj.GetComponent<EnemyHitBox>();
            hitbox.lifespan = 3f;

            hitbox.UpdateFunc = () =>
            {
                proj.transform.position += (Vector3)(dir * speed * Time.deltaTime);
            };

            hitbox.HitEffect = DealDamage;

            hitbox.OnDestroy = () =>
            {
                EnemyProjectileManager.Instance.ReturnToPool(proj, "enemyProjectile");
            };

            attackCooldown = enemyHandler.enemyData.AtkSpeed;

            SetAnimationDirection(dir);
        }
    }

    protected override void Movement()
    {
    }

    private void DealDamage()
    {
        Debug.Log("Enemy projectile hit player");
    }

    private void SetAnimationDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int directionIndex = GetDirectionIndex(angle);

        animator.SetInteger("DirectionIndex", directionIndex);
    }

    private int GetDirectionIndex(float angle)
    {
        angle = (angle + 360f) % 360f;

        if (angle >= 337.5f || angle < 22.5f) return 3;
        if (angle >= 22.5f && angle < 67.5f) return 5;
        if (angle >= 67.5f && angle < 112.5f) return 0;
        if (angle >= 112.5f && angle < 157.5f) return 4;
        if (angle >= 157.5f && angle < 202.5f) return 2;
        if (angle >= 202.5f && angle < 247.5f) return 6;
        if (angle >= 247.5f && angle < 292.5f) return 1;
        if (angle >= 292.5f && angle < 337.5f) return 7;
        return 1; 
    }
}   
