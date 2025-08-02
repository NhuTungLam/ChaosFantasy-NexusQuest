using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldEnemy : EnemyMovement
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
        if (!isAttacking)
            StartCoroutine(AtkSeq(player));
    }
    private IEnumerator AtkSeq(Transform target)
    {
        isAttacking = true;
        animator.Play("shield_enemy_prepare");
        yield return new WaitForSeconds(0.8f);
        Vector2 dir = (target.position - transform.position).normalized;
        if (dir.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dir.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        float chargeTime = 0.5f;
        while (chargeTime > 0)
        {
            transform.position += 12f * Time.deltaTime * (Vector3)dir;
            chargeTime -= Time.deltaTime;
            yield return null;
        }

        animator.Play("shield_enemy_run");
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
        rb.MovePosition(rb.position + (dir * 2f * Time.fixedDeltaTime));
    }
}
