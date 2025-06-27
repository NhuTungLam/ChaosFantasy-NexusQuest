using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IInteractable
{
    public float damage;
    public float cooldown;
    protected float interval;
    protected Animator animator;
    public bool isEquipped = false;
    public string weaponName;
    public float manaCost;
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (interval < cooldown)
        {
            interval += Time.deltaTime;
        }
    }
    public abstract void Attack(CharacterHandler user);

    // ---- IInteractable ----
    public bool CanInteract()
    {
        return  !isEquipped;
    }

    public void Interact(CharacterHandler user=null)
    {
        if (isEquipped || user == null) { return; }
        isEquipped = true;
        user.EquipWeapon(this);
    }
    
    public void InRangeAction(CharacterHandler user = null)
    {
        if (isEquipped || user == null) { return; }
        DungeonPickup.ShowPickup(weaponName, transform.position);
    }
    public void CancelInRangeAction(CharacterHandler user = null)
    {
        if (isEquipped || user == null) { return; }
        DungeonPickup.HidePickup();
    }
}
