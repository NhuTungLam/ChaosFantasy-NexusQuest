using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    private ChestData data;
    public WeaponData[] weaponItems;

    public void ApplyData(ChestData chestData)
    {
        data = chestData;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        isOpen = true;
        Debug.Log("Chest opened: " + data?.name);
        SpawnLoot();
        Destroy(gameObject, 0.2f);
    }

    public bool CanInteract()
    {
        return !isOpen;
    }

    private void SpawnLoot()
    {
        int dropIndex = 0;

        //  Weapon
        if (Random.value < data.weaponDropRate && data.weaponItems.Length > 0)
        {
            WeaponData weapon = data.weaponItems[Random.Range(0, data.weaponItems.Length)];
            Vector3 dropPos = transform.position + GetOffsetByIndex(dropIndex++, 3); 
            GameObject dropped = Instantiate(weapon.weaponPrefab, dropPos, Quaternion.identity);

            if (dropped.TryGetComponent(out PickupWeapon pickup))
            {
                pickup.weaponData = weapon;
            }

            if (dropped.TryGetComponent(out WeaponBase weaponBase))
            {
                weaponBase.weaponData = weapon;
                weaponBase.SetFromData(weapon);
            }
        }

        //Active skill
        if (Random.value < data.activeSkillDropRate && data.activeSkillCards.Length > 0)
        {
            GameObject prefab = data.activeSkillCards[Random.Range(0, data.activeSkillCards.Length)];
            Vector3 dropPos = transform.position + GetOffsetByIndex(dropIndex++, 3);
            Instantiate(prefab, dropPos, Quaternion.identity);
        }

        // Passive skill
        if (Random.value < data.passiveSkillDropRate && data.passiveSkillCards.Length > 0)
        {
            GameObject prefab = data.passiveSkillCards[Random.Range(0, data.passiveSkillCards.Length)];
            Vector3 dropPos = transform.position + GetOffsetByIndex(dropIndex++, 3);
            Instantiate(prefab, dropPos, Quaternion.identity);
        }
        // Other Items (e.g., Health Potion)
        if (Random.value < data.otherItemDropRate && data.otherItems.Length > 0)
        {
            GameObject prefab = data.otherItems[Random.Range(0, data.otherItems.Length)];
            Vector3 dropPos = transform.position + GetOffsetByIndex(dropIndex++, 4);
            Instantiate(prefab, dropPos, Quaternion.identity);
        }

    }
    private Vector3 GetOffsetByIndex(int index, int total, float radius = 1f)
    {
        float angle = 360f / total * index;
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
    }

}
