using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Passive Skill Card")]
public class PassiveSkillCard : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public string description;
    public GameObject skillEffectPrefab;
    public float bonusDamage;
    public float bonusSpeed;
    public float bonusCritRate;
    public float bonusCritDamage;
}
