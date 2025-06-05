using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MeleeEnemy : EnemyMovement
{
    [SerializeField] private Transform target;
    private float attackCD;

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
            float speed = 0f;

            photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "enemyslash",
                transform.position,
                dir,
                speed,
                1f,
                enemyHandler.currentDamage
            );

            attackCD = enemyHandler.enemyData.AtkSpeed;
        }

    }
    private void DealDamage()
    {
        Debug.Log("loss hp");
    }
    protected override void Movement()
    {
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
        rb.MovePosition(rb.position + (dir * 6f * Time.deltaTime));
    }
}
