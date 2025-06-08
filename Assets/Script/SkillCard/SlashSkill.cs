using UnityEngine;

public class SlashSkill : MonoBehaviour, IPassiveSkill
{
    private CharacterHandler player;
    public GameObject slashProjectilePrefab;
    public float damageMultiplier = 1.0f;

    public void Initialize(CharacterHandler player)
    {
        this.player = player;

        // Hook vào hệ thống vũ khí
        if (player.weaponHolder.childCount > 0)
        {
            var weapon = player.weaponHolder.GetChild(0).GetComponent<MeleeWeapon>();
            if (weapon != null)
                weapon.onAttack += OnWeaponAttack; 
            Debug.Log("attackpassive");
        }
    }
    private float cd;
    public void Activate(CharacterHandler player)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && cd > 2)
        {
            OnWeaponAttack();
            cd = 0;
        }
    }
    public void Tick()
    {
        if (cd <2)
        cd += Time.deltaTime;
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnWeaponAttack();
            cd = 0;
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
