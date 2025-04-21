using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public float baseDamage = 10f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    public GameObject projectilePrefab;
    public Transform firePoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        Debug.Log("Attacking!");
        if (projectilePrefab)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        }
        // hoặc chạm gần nếu là melee
    }
}
