using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Staff_F : WeaponBase
{
    public Transform firePoint;
    public int bulletCount = 10;
    public float spreadAngle = 45f; // total arc

    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine) return;
        if (transform.parent != null) transform.rotation = Quaternion.identity;
        if (interval > 0) interval -= Time.deltaTime;
    }

    public override void Attack(CharacterHandler user)
    {
        if (interval > 0) return;
        if (!user.UseMana(manaCost)) return;

        interval = cooldown;

        Vector2 direction = firePoint.right;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = Random.Range(-spreadAngle, spreadAngle);
            float speed = Random.Range(3f, 8f);
            Vector2 rotatedDir = direction.Rotate(angle);
            float lifeSpan = Random.Range(0.5f, 2f);

            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "spell_3",
                firePoint.position,
                rotatedDir.normalized,
                speed,
                lifeSpan,
                user.currentMight + damage,
                0
            );
        }
    }

}
