using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_IncreaseDmg : SkillCardBase
{
    public override void Initialize(CharacterHandler player)
    {
        base.Initialize(player);
        player.currentMight += 10;
    }
    public override void OnRemoveSkill(CharacterHandler player)
    {
        base.OnRemoveSkill(player);
        player.currentMight -= 10;
    }
}
