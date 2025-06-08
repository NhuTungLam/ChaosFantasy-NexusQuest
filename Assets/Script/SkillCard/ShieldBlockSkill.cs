using UnityEngine;
using System.Collections;

public class ShieldBlockSkill : ActiveSkillBase
{
    public float duration = 3f;
    public GameObject shieldEffectPrefab;

    public override void Activate(CharacterHandler player)
    {
        player.StartCoroutine(BlockRoutine(player));
    }

    private IEnumerator BlockRoutine(CharacterHandler player)
    {
        player.isBlocking = true;

        // Instantiate shield effect
        GameObject effect = null;
        if (shieldEffectPrefab != null)
        {
            effect = GameObject.Instantiate(shieldEffectPrefab, player.transform);
            effect.transform.localPosition = Vector3.zero;
        }

        yield return new WaitForSeconds(duration);

        player.isBlocking = false;

        if (effect != null)
            GameObject.Destroy(effect);
    }
}
