using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    private List<GameObject> spawnedEnemies = new();
    private Queue<EnewaveData> roomEnemiesData = new();
    public bool AllEnemiesDefeated => spawnedEnemies.Count == 0 && roomEnemiesData.Count ==0;
    private Vector3 spawnPos = Vector3.zero;
    //pooling
    public GameObject EnemyPrefab;
    private Queue<GameObject> enemiesQueue = new();
    public void CreatePool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var pref= Instantiate(EnemyPrefab);
            pref.SetActive(false);
            enemiesQueue.Enqueue(pref);
        }
    }
    public GameObject GetFromPool()
    {
        if (enemiesQueue.Count == 0)
        {
            CreatePool(2);
            return GetFromPool();
        }
        var enemy = enemiesQueue.Dequeue();
        enemy.SetActive(true);
        return enemy;
    }
    public void ReturnToPool(GameObject e)
    {
        e.SetActive(false);
        enemiesQueue.Enqueue(e);
        spawnedEnemies.Remove(e);
    }
    private void Awake()
    {
        Instance = this;
        
        
    }
    private void Update()
    {
       if(spawnedEnemies.Count == 0 && roomEnemiesData.Count > 0)
        {
            SpawnEnemies();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            var enemy = spawnedEnemies[Random.Range(0,spawnedEnemies.Count)];
            enemy.GetComponent<EnemyHandler>().TakeDamage(99);
        }
    }
    public void SpawnEnemies()
    {
        var currentWave = roomEnemiesData.Dequeue();
        for (int i = 0; i < currentWave.enemycount.Count; i++)
        {
            for (int j = 0; j < currentWave.enemycount[i]; j++)
            {
                var enemy = GetFromPool();
                enemy.transform.position = spawnPos + new Vector3(Random.Range(-3, 3), Random.Range(-3, 3));
                enemy.GetComponent<EnemyHandler>().Init(currentWave.enemylist[i]);
                spawnedEnemies.Add(enemy);
            }
        }
   
    }
    public void RoomSpawner(List<EnewaveData> enemies,Vector3 position) { 
        roomEnemiesData = new(enemies);
        spawnPos = position;
        SpawnEnemies();

    }
}