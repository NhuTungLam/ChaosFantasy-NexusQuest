using Photon.Pun;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviourPunCallbacks, IInteractable
{
    public string weaponId; // unique id
    public float damage;
    public float cooldown;
    protected float interval;
    protected Animator animator;
    public bool isEquipped = false;
    public string weaponName;
    public string prefabName;
    public float manaCost;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(string id)
    {
        weaponId = id;
    }

    public bool CanInteract()
    {
        return !isEquipped;
    }

    public virtual void Interact(CharacterHandler user = null)
    {
        if (isEquipped || user == null) return;
        isEquipped = true;
        int viewID = user.photonView.ViewID;
        string dropWeaponType = user.currentWeapon != null ? user.currentWeapon.prefabName : "";

        // G?i request lên MasterClient ?? x? lý nh?t + drop
        PhotonView photonView = WeaponSyncManager.Instance.photonView;
        photonView.RPC("RPC_RequestPickupWeapon", RpcTarget.MasterClient, weaponId, viewID, prefabName, dropWeaponType, user.transform.position);
    }


    public abstract void Attack(CharacterHandler user);

    public void InRangeAction(CharacterHandler user = null)
    {
        if (isEquipped || user == null) return;
        DungeonPickup.ShowPickup(weaponName, transform.position);
    }

    public void CancelInRangeAction(CharacterHandler user = null)
    {
        if (isEquipped || user == null) return;
        DungeonPickup.HidePickup();
    }
}
