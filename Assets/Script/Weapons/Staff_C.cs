using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Staff_C : WeaponBase
{
    public Transform firePoint;
    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine)
            return;

        if (transform.parent != null)
        {
            //RotateTowardMouse();
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
        StartCoroutine(AtkSeq(user));
        
    }
    private IEnumerator AtkSeq(CharacterHandler user)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 shootDir = (mousePos - transform.position).normalized;
            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "spell_1",
                firePoint.position,
                shootDir,
                5f,
                5f,
                user.currentMight + damage,
                2
            );
            yield return new WaitForSeconds(0.5f);
        }
    }



}
