// ObjectController.cs (s?a l?i ?? phù h?p 1 prefab dùng nhi?u enemyData)
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public EnemyHandler enemyBasePrefab;
    public DamagePopUp popUpPrefab;

    void Awake()
    {
        ObjectPools.ClearPools();

        ObjectPools.SetupPool(enemyBasePrefab, 20, "EnemyBase");

        ObjectPools.SetupPool(popUpPrefab, 10, "DamagePopUp");
    }
}
