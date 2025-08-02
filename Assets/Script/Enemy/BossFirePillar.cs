using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFirePillar : MonoBehaviour
{
    public GameObject firePillar;
    private float interval = 0.2f;

    // Update is called once per frame
    void Update()
    {
        interval -= Time.deltaTime;
        if (interval <= 0)
        {
            var ins = Instantiate(firePillar, transform.position, Quaternion.identity);
            ins.GetComponent<EnemyHitBox>().OnDestroy = () => Destroy(ins);
            ins.GetComponent<EnemyHitBox>().HitEffect = (handler) =>
            {
                //Debug.Log("Projectile hit player");
                handler.TakeDamage(5);
            };
            interval = 0.2f;
        }
    }
}
