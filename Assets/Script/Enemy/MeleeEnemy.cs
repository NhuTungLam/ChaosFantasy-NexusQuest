using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MeleeEnemy : EnemyMovement
{
    private float attackCD;
    private Animator animator;
    private bool isAttacking = false;
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
        if (Vector2.Distance(player.position, transform.position) <= 2)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            if (!isAttacking)
                StartCoroutine(AtkSeq(dir));
        }

    }
    private IEnumerator AtkSeq(Vector2 dir)
    {
        isAttacking = true;
        animator.Play("melee_enemy_lounge");
        yield return new WaitForSeconds(0.7f);
        //Vector2 dir = (target.position - transform.position).normalized;
        float speed = 0f;

        photonView.RPC("RPC_FireProjectile", RpcTarget.All,
            "melee_slash",
            transform.position,
            dir,
            speed,
            0.5f,
            enemyHandler.currentDamage,
            dir.x < 0 ? 1 : 0
        );
        yield return new WaitForSeconds(0.6f);
        attackCD = enemyHandler.enemyData.AtkSpeed;
        isAttacking = false;
    }

    protected override void Movement()
    {
        if (isAttacking) return;
        if (player == null || transform == null || rb == null) return;
        Vector2 dir = player.position - transform.position;

        if (dir.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dir.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        float distance = attackCD > 0 ? 3.5f : 2f;
        dir.Normalize();
        rb.MovePosition(rb.position + (dir * 3f * Time.fixedDeltaTime));
    }
}