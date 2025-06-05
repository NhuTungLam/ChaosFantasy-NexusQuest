using UnityEngine;

public class PickupWeapon : MonoBehaviour, IInteractable
{
    public WeaponData weaponData;
    private bool isPicked = false;

    public bool CanInteract()
    {
        return !isPicked && weaponData != null;
    }

    public void Interact()
    {
        if (isPicked || weaponData == null) return;

        CharacterHandler player = FindObjectOfType<CharacterHandler>();
        if (player != null)
        {
            player.EquipWeapon(weaponData);
            isPicked = true;
            Destroy(gameObject);
        }
    }
}
