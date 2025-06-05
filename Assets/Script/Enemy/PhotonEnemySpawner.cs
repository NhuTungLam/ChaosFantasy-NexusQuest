using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PhotonEnemySpawner : MonoBehaviourPunCallbacks
{
    public string enemyPrefabPath = "Enemies/Enemy1";
    public static PhotonEnemySpawner Instance;

    void Awake() => Instance = this;

    public void SpawnWave(List<EnewaveData> waveList, int index, Vector3 position)
    {
        Debug.Log($"[Spawner] RoomSessionManager.Instance = {RoomSessionManager.Instance}");

        if (!RoomSessionManager.Instance || !RoomSessionManager.Instance.IsRoomOwner())
        {
            Debug.LogWarning("[Spawner] Not RoomOwner, skipping spawn.");
            return;
        }


        if (index >= 0 && index < waveList.Count)
        {
            var wave = waveList[index];
            for (int i = 0; i < wave.enemylist.Count; i++)
            {
                int count = wave.enemycount[i];
                string enemyName = wave.enemylist[i].name;

                for (int j = 0; j < count; j++)
                {
                    Vector3 spawnPos = position + new Vector3(Random.Range(-3, 3), Random.Range(-2, 2), 0);
                    object[] data = new object[] { enemyName };
                    PhotonNetwork.Instantiate(enemyPrefabPath, spawnPos, Quaternion.identity, 0, data);
                    Debug.Log($"[Spawner] Spawned enemy {enemyName} at {spawnPos}");
                }
            }
        }
    }

    public bool AllEnemiesDefeated => GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
}