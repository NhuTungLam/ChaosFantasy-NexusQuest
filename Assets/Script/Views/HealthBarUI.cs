using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider hpSlider;
    public Slider manaSlider;
    public CharacterHandler character;

    private bool initialized = false;

    void Update()
    {
        if (character != null && !initialized)
        {
            if (hpSlider != null)
                hpSlider.maxValue = character.characterData.MaxHealth;

            if (manaSlider != null)
                manaSlider.maxValue = character.characterData.MaxMana;

            initialized = true;
        }

        if (character != null)
        {
            if (hpSlider != null) hpSlider.value = character.currentHealth;
            if (manaSlider != null) manaSlider.value = character.currentMana;
        }
    }
}
