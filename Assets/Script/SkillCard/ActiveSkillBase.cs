using UnityEngine;

public abstract class ActiveSkillBase : MonoBehaviour, IActiveSkill
{
    public abstract void Activate(CharacterHandler player);
}
