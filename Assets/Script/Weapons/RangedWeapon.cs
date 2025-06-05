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
        if (user.currentMana < weaponData.manaCost)
        {
            Debug.Log("[Weapon] Not enough mana.");
            return;
        }

        user.currentMana -= weaponData.manaCost;
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;

        if (weaponData.useFullBodyAnimation && user != null)
        {
            Animator userAnimator = user.GetComponent<Animator>();
            if (userAnimator != null)
                userAnimator.SetTrigger("Attack");

            gameObject.SetActive(false);
            user.StartCoroutine(ReenableWeaponAfter(user, weaponData.delayBeforeFire + 0.3f)); // tùy chỉnh

            Vector2 shootDir = firePoint.right;
            user.StartCoroutine(user.FireProjectileDelayed(
                firePoint,
                shootDir,
                weaponData.delayBeforeFire,
                projectilePrefab,
                damage
            ));

        }
        else if (animator != null)
        {
            animator.SetTrigger("Attack");

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mouseWorld - (Vector2)firePoint.position).normalized;

            proj.GetComponent<Projectile>().Initialize(direction, damage);
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
    private System.Collections.IEnumerator ReenableWeaponAfter(CharacterHandler user, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(true);
    }


}
