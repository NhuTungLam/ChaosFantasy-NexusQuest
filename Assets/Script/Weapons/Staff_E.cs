using Photon.Pun;
using UnityEngine;

public class Staff_E : WeaponBase
{
    //public Transform firePoint;
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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 shootDir = (mousePos - transform.position).normalized;
        user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
            "spell_2",
            (Vector3)mousePos,
            Vector2.one,
            0f,
            0.7f,
            user.currentMight + damage,
            0
        );
    }

}
