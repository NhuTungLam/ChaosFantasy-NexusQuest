using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_ShieldBlock : SkillCardBase
{
    private float interval;
    public float duration;
    public GameObject shieldEffectPrefab;

    public override void Initialize(CharacterHandler player)
    {
        base.Initialize(player);
        if (duration > cooldown) 
            duration = cooldown;
    }
    private void Update()
    {
        if (interval < cooldown)
            interval += Time.deltaTime;
    }
    public override void Activate(CharacterHandler player)
    {
        if (interval < cooldown) return;

        player.isBlocking = true;
        GameObject effect = GameObject.Instantiate(shieldEffectPrefab, player.transform);
        effect.transform.localPosition = Vector3.zero;

        this.Invoke(() =>
        {
            player.isBlocking = false;
            Destroy(effect);
        }, duration);
        interval = 0;
    }
}
