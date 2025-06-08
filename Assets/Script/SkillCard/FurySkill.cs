using UnityEngine;

public class FurySkill : MonoBehaviour, IPassiveSkill
{
    private CharacterHandler player;
    private bool isActive = false;

    public float furyThreshold = 0.3f;
    public float bonusDamagePercent = 0.25f;
    public float damageReductionPercent = 0.2f;

    public void Initialize(CharacterHandler player)
    {
        this.player = player;
    }

    public void Tick()
    {
        if (player == null) return;

        float hpPercent = player.GetCurrentHealthPercent();

        if (!isActive && hpPercent <= furyThreshold)
        {
            isActive = true;
            player.currentMight *= (1 + bonusDamagePercent);
            // player.damageReduction += damageReductionPercent;
        }
        else if (isActive && hpPercent > furyThreshold)
        {
            isActive = false;
            player.currentMight /= (1 + bonusDamagePercent);
            // player.damageReduction -= damageReductionPercent;
        }
    }
}
