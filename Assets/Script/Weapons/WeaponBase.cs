using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public float damage;
    public float cooldown;
    protected float nextAttackTime;

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public abstract void Attack(CharacterHandler user);
}
