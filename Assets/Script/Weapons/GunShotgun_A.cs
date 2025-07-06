using System.Collections;
using Photon.Pun;
using UnityEngine;

public class GunShotgun_A : WeaponBase
{
    public Transform firePoint;
    public int bulletCount = 5;
    public float spreadAngle = 45f; // total arc

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

        Vector2 direction = firePoint.right;
        float angleStep = spreadAngle / (bulletCount - 1);
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector2 rotatedDir = direction.Rotate(angle);

            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "bullet_0",
                firePoint.position,
                rotatedDir.normalized,
                30f,
                0.15f,
                user.currentMight + damage,
                2
            );
        }
    }

    private void RotateTowardMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float flip = (angle > -89f && angle < 89f) ? 1f : -1f;
        Vector3 original = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(original.x) * flip, Mathf.Abs(original.y) * flip, original.z);
    }
}
