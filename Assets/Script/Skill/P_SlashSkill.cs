using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_SlashSkill : SkillCardBase
{
    private CharacterHandler player;
    public GameObject slashProjectilePrefab;
    public float damageMultiplier = 1.0f;

    public override void Initialize(CharacterHandler player)
    {
        base.Initialize(player);
        this.player = player;

        if (player.weaponHolder.childCount > 0)
        {
            var weapon = player.weaponHolder.GetChild(0).GetComponent<MeleeWeapon>();
            if (weapon != null)
                weapon.onAttack += OnWeaponAttack;
            Debug.Log("attackpassive");
            //player.currentMight += 5; cong chi so
        }
    }
    public override void OnRemoveSkill(CharacterHandler player)
    {
        if (player.weaponHolder.childCount > 0)
        {
            var weapon = player.weaponHolder.GetChild(0).GetComponent<MeleeWeapon>();
            if (weapon != null)
                weapon.onAttack -= OnWeaponAttack;
            //player.currentMight += 5; cong chi so
        }
    }
    private void OnWeaponAttack()
    {
        Debug.Log("wweapon slash atttack");
        Transform weapon = player.weaponHolder.childCount > 0 ? player.weaponHolder.GetChild(0) : player.weaponHolder;
        Transform spawnPoint = weapon.Find("SpawnPoint");

        Vector2 direction = player.lastMoveDirection.normalized;
        float damage = player.currentMight * damageMultiplier;

        if (Random.value <= player.currentCritRate)
            damage *= player.currentCritDamage;

        GameObject proj = Instantiate(slashProjectilePrefab, spawnPoint != null ? spawnPoint.position : weapon.position, Quaternion.identity);
        proj.GetComponent<Projectile>().Initialize(direction, damage);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
