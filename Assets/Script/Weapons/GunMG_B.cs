using Photon.Pun;
using UnityEngine;
using System.Collections;

public class GunMG_B : WeaponBase
{
    public Transform firePoint;

    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine) return;
        if (transform.parent != null) RotateTowardMouse();
        if (interval > 0) interval -= Time.deltaTime;
    }

    public override void Attack(CharacterHandler user)
    {
        if (interval > 0) return;
        if (!user.UseMana(manaCost)) return;

        interval = cooldown;
        StartCoroutine(AtkSeq(user));
    }

    private IEnumerator AtkSeq(CharacterHandler user)
    {
        int shots = 5;
        for (int i = 0; i < shots; i++)
        {
            float angleOffset = Random.Range(-5f, 5f);
            Vector2 shootDir = firePoint.right.ToVector2().Rotate(angleOffset);
            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "bullet_1",
                firePoint.position,
                shootDir,
                14f,
                1.2f,
                user.currentMight + damage,
                2
            );
            yield return new WaitForSeconds(0.2f);
        }
    }

    void RotateTowardMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float flip = (angle > -89f && angle < 89f) ? 1f : -1f;
        Vector3 original = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(original.x) * flip, Mathf.Abs(original.y) * flip, original.z);
    }
}
