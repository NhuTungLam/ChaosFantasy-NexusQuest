using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Fury : SkillCardBase
{
    [Header("Fury Data")]
    public float damageBoost = 1.5f; // Damage multiplier when active
    public float damageReduction = 0.5f; // Damage reduction when active
    public float healthThreshold = 0.3f; // Health threshold for activation
    public bool isFuryActive = false;
    private CharacterHandler player;

    // Initialize the Fury skill card
    public override void Initialize(CharacterHandler player)
    {
        base.Initialize(player);
        this.player = player;

        player.OnBeforeTakeDamage += (float dmg) =>
        {
            if (GetCurrentHpPercent() < 0.30f && isFuryActive == false)
            {
                float boost = player.baseMight * damageBoost;
                player.currentMight += boost;
                isFuryActive=true;
                Debug.Log("ffury active");
            }
            if (GetCurrentHpPercent() > 0.30f && isFuryActive == true)
            {
                float boost = player.baseMight * damageBoost;
                player.currentMight -= boost;
                isFuryActive=false;
                Debug.Log(("fury deactive"));
            }
            if (GetCurrentHpPercent() < 0.30f)
            {
                dmg *= (1 - damageReduction);
            }
            return dmg;
        };

        Debug.Log("P_Fury Activated: Increased damage and reduced damage taken.");
    }

    private float GetCurrentHpPercent()
    {
        return player.currentHealth / player.characterData.MaxHealth;
    }
}

