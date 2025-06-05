using Photon.Pun;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StationaryRangedEnemy : EnemyMovement
{
    [SerializeField] private Transform target;
    private float attackCooldown;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>(); 
    }

    protected override void Attack()
    {

        Vector2 dir1 = (player.position - transform.position).normalized;
        SetAnimationDirection(dir1);
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            return;
        }
        if (player == null || player.transform == null || enemyHandler == null)
        {
            Debug.LogError("Player or EnemyHandler is not assigned!");
            return;
        }
        if (Vector2.Distance(player.position, transform.position) <= 7)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            float speed = enemyHandler.enemyData.ProjectileSpeed;
            float atkSpeed = enemyHandler.enemyData.AtkSpeed;

            photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "enemyProjectile",
                transform.position,
                dir,
                speed,
                3f,
                enemyHandler.currentDamage
            );

            attackCooldown = atkSpeed;
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
        direction.Normalize();
        animator.SetFloat("X",direction.x); 
        animator.SetFloat("Y",direction.y);

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
