using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Active Skill Card")]
public class ActiveSkillCard : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public string description;
    public GameObject skillEffectPrefab;
    public float cooldown;
}
