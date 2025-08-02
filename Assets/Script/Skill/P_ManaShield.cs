using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class P_ManaShield : SkillCardBase
{
    public float manaAbsorbPercent = 0.3f;
    private CharacterHandler player;
    public override void Initialize(CharacterHandler player)
    {
        base.Initialize(player);
        this.player = player;
        player.OnBeforeTakeDamage +=  RedirectDamageToMana;
    }
    public override void OnRemoveSkill(CharacterHandler player)
    {
        base.OnRemoveSkill(player);
        player.OnBeforeTakeDamage -= RedirectDamageToMana;
    }
    private float RedirectDamageToMana(float incomingDamage)
    {
        float manaAbsorb = incomingDamage * manaAbsorbPercent;

        if (player.UseMana(manaAbsorb))
            return incomingDamage - manaAbsorb;
        else
        {
            manaAbsorb = player.currentMana;
            player.UseMana(manaAbsorb);
            return incomingDamage - manaAbsorb;
        }
        
    }
}
