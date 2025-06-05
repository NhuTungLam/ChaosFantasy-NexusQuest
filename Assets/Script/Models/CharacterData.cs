using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/Character")]
public class CharacterData : ScriptableObject
{
    [SerializeField]
    private Sprite playerSprite;
    public Sprite PlayerSprite { get => playerSprite; private set => playerSprite = value; }

    [SerializeField]
    private RuntimeAnimatorController animationController;
    public RuntimeAnimatorController AnimationController { get => animationController; private set => animationController = value; }

    [SerializeField]
    public WeaponData startingWeapon;
    public WeaponData StartingWeapon { get => startingWeapon; private set => startingWeapon = value; }

    [SerializeField]
    private float maxHealth;
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    [SerializeField]
    private float maxMana;
    public float MaxMana { get => maxMana; set => maxMana = value; }

    [SerializeField]
    private float moveSpeed;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    [SerializeField]
    private float recovery;
    public float Recovery { get => recovery; set => recovery = value; }

    [SerializeField]
    private float might;
    public float Might { get => might; set => might = value; }

    [SerializeField]
    private float projectileSpeed;
    public float ProjectileSpeed { get => projectileSpeed; set => projectileSpeed = value; }

    [SerializeField]
    private float magnet;
    public float Magnet { get => magnet; set => magnet = value; }

    [SerializeField]
    private float cooldownReduction;
    public float CooldownReduction { get => cooldownReduction; set => cooldownReduction = value; }
}
