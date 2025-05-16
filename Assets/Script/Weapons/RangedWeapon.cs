using Photon.Pun;
using UnityEngine;

public class RangedWeapon : WeaponBase
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    void Update()
    {
        if (transform.parent != null) 
        {
            RotateTowardMouse();
        }
    }


    public override void Attack(CharacterHandler user)
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;

        if (animator != null)
            animator.SetTrigger("Attack");
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = proj.GetComponent<Projectile>();
        projectile.Initialize(firePoint.right, damage); 

    }

    void RotateTowardMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float flip = (angle > -89f && angle < 89f) ? 1f : -1f;
        transform.localScale = new Vector3(flip, flip, 1f);
    }


}
