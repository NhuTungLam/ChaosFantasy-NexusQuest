using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Chest Data")]
public class ChestData : ScriptableObject
{
    public GameObject chestPrefab;
    public GameObject[] weaponItems;
    public GameObject[] passiveSkillCards;
    public GameObject[] activeSkillCards;
    public GameObject[] otherItems;

    [Range(0, 1)] public float weaponDropRate = 0.4f;
    [Range(0, 1)] public float passiveSkillDropRate = 0.3f;
    [Range(0, 1)] public float activeSkillDropRate = 0.2f;
    [Range(0, 1)] public float otherItemDropRate = 0.1f;
}
