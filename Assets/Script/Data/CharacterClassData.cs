using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "ChaosFantasy/Character Class")]
public class CharacterClassData : ScriptableObject
{
    public string className;
    public Sprite classIcon;

    [Header("Stats")]
    public float maxHP;
    public float maxMana;
    public float moveSpeed;
    public float attackSpeed;

    [Header("Starting Equipment")]
    public GameObject defaultWeapon;
    public GameObject[] startingSkills;
}
