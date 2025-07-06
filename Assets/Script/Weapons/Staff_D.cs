using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Staff_D : WeaponBase
{
    public Transform firePoint;
    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine)
            return;

        if (transform.parent != null)
        {
            transform.rotation = Quaternion.identity;
        }
        if (interval > 0)
        {
            interval -= Time.deltaTime;
        }
    }

    public override void Attack(CharacterHandler user)
    {
        if (interval > 0)
            return;
        if (!user.UseMana(manaCost))
        {
            //Debug.Log("[Weapon] Not enough mana.");
            return;
        }
        interval = cooldown;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootDir = (mousePos - transform.position).normalized;
        for (int i = 0; i < 6; i++)
        {
            var dir = shootDir.Rotate(60 * i);
            var speed = 10f;
            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "spell_4",
                firePoint.position,
                dir,
                speed,
                2f,
                user.currentMight + damage,
                shootDir.x > 0? 0 : 1
            );
        }
    }

}
