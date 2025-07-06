using Photon.Pun;
using UnityEngine;

public class GunMG_A : WeaponBase
{
    public Transform firePoint;
    public float offsetDistance = 0.3f;
    public float overheatCD = 2;
    private int overheatCharge = 0;

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
        if (!user.UseMana(manaCost))
        {
            Debug.Log("[GunMG_A] Not enough mana.");
            return;
        }
        overheatCharge += 1;
        interval = cooldown;
        if (overheatCharge == 10)
        {
            interval = overheatCD;
            overheatCharge = 0;
        }
        FireDualStream(user);
        
    }

    private void FireDualStream(CharacterHandler user)
    {
        Vector2 forward = firePoint.right.normalized;
        Vector2 perp = Vector2.Perpendicular(forward);

        Vector2 leftFirePos = (Vector2)firePoint.position + perp * offsetDistance;
        Vector2 rightFirePos = (Vector2)firePoint.position - perp * offsetDistance;

        Fire(user, leftFirePos, forward);
        Fire(user, rightFirePos, forward);
    }

    private void Fire(CharacterHandler user, Vector3 position, Vector2 direction)
    {
        user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
            "bullet_0",              // small bullet
            position,
            direction.normalized,
            16f,                     // speed
            1.2f,                    // lifespan
            user.currentMight + damage,
            2                        // rotation mode (angle based)
        );
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
