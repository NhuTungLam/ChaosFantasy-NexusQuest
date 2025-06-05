using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite weaponSprite;
    public RuntimeAnimatorController animatorController;
    public float damage;
    public float cooldown;
    public float range;
    public GameObject weaponPrefab;
    public WeaponType type;
    public bool useFullBodyAnimation = false;
    [Range(0f, 1f)]
    public float delayBeforeFire = 0.3f;
    [Header("Mana")]
    public float manaCost = 1f;

}

public enum WeaponType
{
    Melee,
    Ranged
}
