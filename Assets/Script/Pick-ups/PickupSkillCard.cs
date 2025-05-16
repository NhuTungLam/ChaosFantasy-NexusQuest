public class PickupSkillCard : Pickup
{
    public ActiveSkillCard skillData;

    protected override void OnDestroy()
    {
        if (!target) return;

        target.SetActiveSkill(skillData);
    }
}
