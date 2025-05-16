using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : EnemyMovement
{
    private float attackCD;

    protected override void Attack()
    {
        if (attackCD > 0)
        {
            attackCD -= Time.deltaTime;
            return;
        }
        if (Vector2.Distance(player.position, transform.position)<=2)
        {
            var attack = EnemyProjectileManager.Instance.GetFromPool("enemyslash");
            attack.transform.SetParent(transform, false);
            attack.transform.localPosition = Vector2.zero;
            attack.GetComponent<Animator>().Rebind();
            var attackHitBox = attack.GetComponent<EnemyHitBox>();
            attackHitBox.lifespan = 1f;
            attackHitBox.HitEffect= DealDamage;
            attackHitBox.OnDestroy = () => 
            {
                EnemyProjectileManager.Instance.ReturnToPool(attack,"enemyslash");
            };
            attackCD = enemyHandler.enemyData.AtkSpeed;
            Debug.Log("Enemy attack");
        }
    }
    private void DealDamage()
    {
        Debug.Log("loss hp");
    }
    protected override void Movement()
    {
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
