using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    public float attackRange;
    public LayerMask hitMask;

    public override void Attack(CharacterHandler user)
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;

        if (animator != null)
            animator.SetTrigger("Attack");

        Vector2 origin = user.transform.position + user.transform.right * (attackRange / 2);
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRange, hitMask);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damage);
            }
        }
    }

}
