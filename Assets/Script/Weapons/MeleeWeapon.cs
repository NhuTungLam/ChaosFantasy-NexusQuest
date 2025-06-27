using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    public float attackRange;
    public LayerMask hitMask;
    public GameObject hitboxPrefab;

    public System.Action onAttack; // ?? thêm event

    public override void Attack(CharacterHandler user)
    {
        //if (user.currentMana < weaponData.manaCost)
        //{
        //    Debug.Log("[Weapon] Not enough mana.");
        //    return;
        //}

        //user.currentMana -= weaponData.manaCost;
        //if (Time.time < nextAttackTime) return;
        //nextAttackTime = Time.time + cooldown;

        if (animator != null)
            animator.SetTrigger("Attack");

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorld - user.transform.position).normalized;
        bool isLeft = mouseWorld.x < user.transform.position.x;

        float offset = 1.5f * (isLeft ? -1 : 1)  ;

        Vector3 spawnPos = user.transform.position + new Vector3 (offset,0);

        GameObject go = GameObject.Instantiate(hitboxPrefab, spawnPos, Quaternion.identity);

        if (go.TryGetComponent(out SpriteRenderer sr))
        {
            sr.flipX = isLeft;
        }

        if (go.TryGetComponent(out Projectile proj))
        {
            //proj.Initialize(direction, damage);
        }

        Destroy(go, 0.5f);

        onAttack?.Invoke();
    }
}

