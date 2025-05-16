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
        CharacterHandler player = FindObjectOfType<CharacterHandler>();

        if (Random.value < data.weaponDropRate && data.weaponItems.Length > 0)
        {
            WeaponData weaponData = data.weaponItems[Random.Range(0, data.weaponItems.Length)];
            if (weaponData.weaponPrefab != null)
            {
                GameObject drop = Instantiate(weaponData.weaponPrefab, transform.position, Quaternion.identity);

                WeaponBase weapon = drop.GetComponent<WeaponBase>();
                if (weapon != null)
                {
                    weapon.weaponData = weaponData;
                    weapon.damage = weaponData.damage;
                    weapon.cooldown = weaponData.cooldown;

                    // Không gán Animator ? ?ây
                    // Gán sprite (n?u có) ?? hi?n th?
                    SpriteRenderer sr = weapon.GetComponent<SpriteRenderer>();
                    if (sr != null && weaponData.weaponSprite != null)
                    {
                        sr.sprite = weaponData.weaponSprite;
                    }
                }
            }
        }


        if (Random.value < data.passiveSkillDropRate && data.passiveSkillCards.Length > 0)
            Instantiate(data.passiveSkillCards[Random.Range(0, data.passiveSkillCards.Length)], transform.position, Quaternion.identity);

        if (Random.value < data.activeSkillDropRate && data.activeSkillCards.Length > 0)
            Instantiate(data.activeSkillCards[Random.Range(0, data.activeSkillCards.Length)], transform.position, Quaternion.identity);

        if (Random.value < data.otherItemDropRate && data.otherItems.Length > 0)
            Instantiate(data.otherItems[Random.Range(0, data.otherItems.Length)], transform.position, Quaternion.identity);
    }
}
