using System.Collections;
using Photon.Pun;
using UnityEngine;

public class GunSniper_A : WeaponBase
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
        StartCoroutine(FirePiercingBurst(user));
    }

    private IEnumerator FirePiercingBurst(CharacterHandler user)
    {
        int shotCount = 3;
        float delay = 0.15f;
        // dir = firePoint.right;
        float bulletSpeed = 30f;
        float lifespan = 0.6f;

        for (int i = 0; i < shotCount; i++)
        {
            string bullet = $"bullet_{i}";
            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                bullet,
                firePoint.position,
                (Vector2)firePoint.right,
                bulletSpeed,
                lifespan,
                user.currentMight + damage,
                2
            );
            yield return new WaitForSeconds(delay);
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
