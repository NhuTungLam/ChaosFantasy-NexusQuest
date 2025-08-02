using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BowEnemy : EnemyMovement
{
    [SerializeField] private float preferredRange = 6f;
    [SerializeField] private float rangeBuffer = 1f; // allows for slight leeway before moving
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float strafeSpeed = 3f;
    [SerializeField] private float strafeChangeInterval = 1.5f;

    private float strafeDirection = 1f;
    private float strafeTimer = 0f;
    private bool isAttacking = false;
    private float attackCD;
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    protected override void Attack()
    {
        if (attackCD > 0)
        {
            attackCD -= Time.deltaTime;
            return;
        }
        if (player == null || transform == null) return;

        if (!isAttacking)
            StartCoroutine(AtkSeq(player));
    }
    private IEnumerator AtkSeq(Transform target)
    {
        isAttacking = true;
        animator.Play("bow_enemy_shoot");
        yield return new WaitForSeconds(0.5f);
        Vector2 dir = (target.position - transform.position).normalized;
        float speed = 12f;

        photonView.RPC("RPC_FireProjectile", RpcTarget.All,
            "enemy_arrow",
            transform.position,
            dir,
            speed,
            1f,
            enemyHandler.currentDamage,
            2
        );
        yield return new WaitForSeconds(0.2f);
        attackCD = enemyHandler.enemyData.AtkSpeed;
        isAttacking = false;
    }
    protected override void Movement()
    {
        if (isAttacking || player == null || transform == null || rb == null)
            return;

        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector2 direction = Vector2.zero;

        // Face player
        if (toPlayer.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);

        // Movement logic
        if (distance > preferredRange + rangeBuffer)
        {
            // Too far  move closer
            direction = toPlayer.normalized;
        }
        else if (distance < preferredRange - rangeBuffer)
        {
            // Too close  move away
            direction = -toPlayer.normalized;
        }
        else
        {
            // In optimal range  strafe
            strafeTimer -= Time.deltaTime;
            if (strafeTimer <= 0f)
            {
                strafeDirection = Random.value < 0.5f ? -1f : 1f; // Left or right
                strafeTimer = strafeChangeInterval;
            }

            // Get perpendicular toPlayer direction for strafing (left/right)
            Vector2 perpendicular = Vector2.Perpendicular(toPlayer).normalized;
            direction = perpendicular * strafeDirection;
            moveSpeed = strafeSpeed;
        }

        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * direction);
    }

}
