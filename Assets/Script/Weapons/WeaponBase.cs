using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IInteractable
{
    public float damage;
    public float cooldown;
    protected float nextAttackTime;
    protected Animator animator;
    public bool isEquipped = false;
    public WeaponData weaponData;
    

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public abstract void Attack(CharacterHandler user);

    // ---- IInteractable ----
    public bool CanInteract()
    {
        return weaponData != null && transform.parent == null;
    }

    public void Interact()
    {
        CharacterHandler player = FindObjectOfType<CharacterHandler>();
        if (player != null && CanInteract())
        {
            player.EquipWeapon(weaponData);
            Destroy(gameObject);
        }
    }
    public virtual void SetFromData(WeaponData data)
    {
        weaponData = data;
        damage = data.damage;
        cooldown = data.cooldown;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.weaponSprite != null)
            sr.sprite = data.weaponSprite;

        Animator anim = GetComponent<Animator>();
        if (anim != null && data.animatorController != null)
            anim.runtimeAnimatorController = data.animatorController;
    }

}
