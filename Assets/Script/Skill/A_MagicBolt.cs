using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class A_MagicBolt : SkillCardBase
{
    //public GameObject boltPrefab;
    public float damageMultiplier = 1.2f;
    private float interval;

    private void Update()
    {
        if (interval < cooldown)
            interval += Time.deltaTime;
    }
    public override void Activate(CharacterHandler player)
    {
        if (interval < cooldown) return;

        Shoot(player);
        interval = 0;
    }

    private void Shoot(CharacterHandler player)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - player.transform.position).normalized;

        float damage = player.currentMight * damageMultiplier;
        if (Random.value <= player.currentCritRate)
        {
            damage *= player.currentCritDamage;
            //Debug.Log($"[MagicBolt] CRITICAL! Damage: {damage}");
        }

        player.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
            "spell_skill",
            transform.position,
            direction,
            10f,
            1f,
            damage,
            2);
    }
}
