using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SwordRed : WeaponSword
{
    public override void Attack(CharacterHandler user)
    {
        if (interval > 0) return;
        interval = cooldown;
        returnToIdleTimer = 0f;
        onAttack?.Invoke();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 direction = (mouseWorld - user.transform.position).normalized;
        bool isLeft = mouseWorld.x < user.transform.position.x;

        switch (currentSlashIndex)
        {
            case 0:
                AnimationSlashDownward(0.8f);
                currentSlashIndex = 1;
                user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "slash_red_0",
                transform.position + (isLeft ? -1 : 1) * new Vector3(1.5f, 0),
                Vector2.zero,
                0f,
                0.4f,
                user.currentMight + damage,
                isLeft ? 1 : 0);
                break;
            case 1:
                AnimationSlashUpward(0.8f);
                currentSlashIndex = 0;
                user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "slash_red_1",
                transform.position + (isLeft ? -1 : 1) * new Vector3(1.5f, 0),
                Vector2.zero,
                0f,
                0.4f,
                user.currentMight + damage,
                isLeft ? 1 : 0);
                break;
        }
    }
}
