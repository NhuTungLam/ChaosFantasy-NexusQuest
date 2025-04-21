using ChaosFantasy.Models;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public GameObject skillEffectPrefab;
    public float manaCost = 20f;
    public float cooldown = 5f;
    private float lastUseTime;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time > lastUseTime + cooldown && stats.currentMana >= manaCost)
        {
            UseSkill();
            lastUseTime = Time.time;
        }
    }

    void UseSkill()
    {
        stats.UseMana(manaCost);
        Debug.Log("Skill Activated!");
        Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);
    }
}
