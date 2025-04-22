using UnityEngine;
using Photon.Pun;

public class MultiplayerSpawnManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerNetworked";

    public Vector2 spawnAreaMin = new Vector2(-3, -3);
    public Vector2 spawnAreaMax = new Vector2(3, 3);

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                0f
            );

            PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[MultiplayerSpawnManager] Not in a Photon room. Cannot spawn player.");
        }
    }
}