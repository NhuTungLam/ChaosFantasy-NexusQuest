using UnityEngine;
using UnityEngine.UI;
using ChaosFantasy.Models; // để dùng PlayerStats

public class HealthBarUI : MonoBehaviour
{
    public PlayerStats stats;

    public Slider hpSlider;
    public Slider armorSlider;
    public Slider manaSlider;

    void Start()
    {
        hpSlider.maxValue = stats.maxHP;
        armorSlider.maxValue = stats.maxArmor;
        manaSlider.maxValue = stats.maxMana;
    }

    void Update()
    {
        hpSlider.value = stats.currentHP;
        armorSlider.value = stats.currentArmor;
        manaSlider.value = stats.currentMana;
    }
}
