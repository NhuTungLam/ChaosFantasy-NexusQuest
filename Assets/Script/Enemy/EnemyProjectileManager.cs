using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileManager : MonoBehaviour
{
    public static EnemyProjectileManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    //pooling
    private Dictionary<string,Queue<GameObject>> projectilePools = new Dictionary<string,Queue<GameObject>>();
    public void CreatePool(string projectileName, int count)
    {
        var prefab = Resources.Load<GameObject>("Enemies/" + projectileName);
        if (prefab == null)
        {
            Debug.Log("No prefab find");
            return;

        }
        Queue<GameObject> pool = new Queue<GameObject>();
        if (projectilePools.ContainsKey(projectileName))
        {
            pool = projectilePools[projectileName];
        }
        else
        {
            projectilePools.Add(projectileName, pool);
        }
        for (int i = 0; i < count; i++)
        {
            var instance = Instantiate(prefab,this.transform);
            instance.SetActive(false);
            pool.Enqueue(instance);
        }
    }
    public GameObject GetFromPool(string poolName)
    {
        if (projectilePools.ContainsKey((string)poolName))
        {
            var pool = projectilePools[poolName];
            if (pool.Count == 0)
            {
                CreatePool(poolName, 2);
                return GetFromPool(poolName);
            }
            else
            {
                var proj = pool.Dequeue();
                proj.SetActive(true);
                return proj;
            }
        }
        else 
        {
            CreatePool(poolName, 5);
            return GetFromPool(poolName);
        }
    }
    public void ReturnToPool(GameObject obj, string returnProj)
    {
        var pool = projectilePools[returnProj];
        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        pool.Enqueue(obj);
    }
}
