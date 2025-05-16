using UnityEngine;

public class PickupWeapon : Pickup
{
    public WeaponData weaponData;

    protected override void Update()
    {
        base.Update();

        // Khi ?ang g?n ng??i ch?i
        if (target && knockbackDuration <= 0)
        {
            // Cho phép nh?t khi nh?n E
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (weaponData != null)
                {
                    target.EquipWeapon(weaponData);
                    Destroy(gameObject);
                }
            }
        }
    }
}
