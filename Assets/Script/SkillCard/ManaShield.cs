using UnityEngine;

public class ManaShield : MonoBehaviour, IPassiveSkill
{
    private CharacterHandler player;
    public float manaAbsorbPercent = 0.3f;

    public void Initialize(CharacterHandler player)
    {
        this.player = player;
        player.OnBeforeTakeDamage += RedirectDamageToMana;
    }

    public void Tick() { }

    private float RedirectDamageToMana(float incomingDamage)
    {
        float manaAbsorb = incomingDamage * manaAbsorbPercent;

        if (player.currentMana >= manaAbsorb)
        {
            player.currentMana -= manaAbsorb;
            return incomingDamage - manaAbsorb;
        }
        else
        {
            float remaining = manaAbsorb - player.currentMana;
            player.currentMana = 0;
            return incomingDamage - (manaAbsorb - remaining);
        }
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnBeforeTakeDamage -= RedirectDamageToMana;
    }
}
