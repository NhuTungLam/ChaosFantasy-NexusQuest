using UnityEngine;

public class ShadowClone : MonoBehaviour
{
    public float lifeTime = 4f;
    public float moveSpeed = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    private float attackTimer = 0f;
    private float mana = 1000f;
    private CharacterHandler owner;
    private WeaponBase weapon;
    private Animator animator;

    public void Initialize(CharacterHandler original)
    {
        owner = original;

        // Copy sprite + animator
        SpriteRenderer src = original.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer dst = GetComponentInChildren<SpriteRenderer>();
        if (src != null && dst != null)
        {
            dst.sprite = src.sprite;
            dst.flipX = src.flipX;
        }

        animator = GetComponent<Animator>();
        if (animator != null && original.characterData.AnimationController != null)
        {
            animator.runtimeAnimatorController = original.characterData.AnimationController;
        }

        // Copy weapon
        if (original.weaponData != null)
        {
            WeaponData cloneData = ScriptableObject.CreateInstance<WeaponData>();
            cloneData.weaponPrefab = original.weaponData.weaponPrefab;
            cloneData.damage = original.weaponData.damage * 0.8f;
            cloneData.cooldown = original.weaponData.cooldown;
            cloneData.manaCost = 0f;
            cloneData.animatorController = original.weaponData.animatorController;
            cloneData.weaponSprite = original.weaponData.weaponSprite;

            // Tạo weapon object
            GameObject wp = Instantiate(cloneData.weaponPrefab, transform);
            weapon = wp.GetComponent<WeaponBase>();
            weapon.weaponData = cloneData;
            weapon.damage = cloneData.damage;
            weapon.cooldown = cloneData.cooldown;
            weapon.isEquipped = true;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        }
        else
        {
            
            if (attackTimer <= 0 && weapon != null)
            {
                weapon.Attack(null); // Clone 
                attackTimer = attackCooldown;
            }
        }
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5f, LayerMask.GetMask("Enemy"));
        if (hits.Length == 0) return null;

        Transform nearest = hits[0].transform;
        float minDist = Vector2.Distance(transform.position, nearest.position);

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                nearest = hit.transform;
                minDist = dist;
            }
        }
        return nearest;
    }
}
