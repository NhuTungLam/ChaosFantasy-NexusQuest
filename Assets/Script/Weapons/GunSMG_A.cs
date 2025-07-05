using System.Collections;
using Photon.Pun;
using UnityEngine;

public class GunSMG_A : WeaponBase
{
    public Transform firePoint;
    private bool isFiring = false;

    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine)
            return;

        if (transform.parent != null)
            RotateTowardMouse();

        if (interval > 0)
            interval -= Time.deltaTime;
    }

    public override void Attack(CharacterHandler user)
    {
        if (interval > 0) return;
        if (!user.UseMana(manaCost)) return;

        interval = cooldown;
        StartCoroutine(FireRapid(user));
    }

    private IEnumerator FireRapid(CharacterHandler user)
    {
        int shots = 6;
        while (shots-- > 0)
        {
            Vector2 dir = firePoint.right.ToVector2().Rotate(Random.Range(-10f, 10f));
            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "bullet_0",
                firePoint.position,
                dir,
                20f,
                1f,
                user.currentMight + damage,
                2);

            yield return new WaitForSeconds(0.15f);
        }
    }

    void RotateTowardMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        float flip = (angle > -89f && angle < 89f) ? 1f : -1f;
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x) * flip, Mathf.Abs(s.y) * flip, s.z);
    }
}
