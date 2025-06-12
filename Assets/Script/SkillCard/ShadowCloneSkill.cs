using UnityEngine;

public class ShadowCloneSkill : ActiveSkillBase
{
    public GameObject clonePrefab;

    public override void Activate(CharacterHandler player)
    {
        GameObject clone = Instantiate(clonePrefab, player.transform.position + Vector3.left, Quaternion.identity);
        if (clone.TryGetComponent(out ShadowClone sc))
        {
            sc.Initialize(player);
        }
    }
}
