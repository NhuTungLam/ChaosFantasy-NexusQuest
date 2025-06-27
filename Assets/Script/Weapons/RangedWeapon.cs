using Photon.Pun;
using UnityEngine;

public class RangedWeapon : WeaponBase
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine)
            return;

        if (transform.parent != null)
        {
            RotateTowardMouse();
        }
    }

    public override void Attack(CharacterHandler user)
    {
        if (!user.UseMana(manaCost))
        {
            Debug.Log("[Weapon] Not enough mana.");
            return;
        }
        interval = cooldown;
        Vector2 shootDir = firePoint.right;
        user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
            "Projectile",
            transform.position,
            shootDir,
            10f,
            1f,
            user.currentMight + damage,
            0
        );
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
