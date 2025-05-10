using UnityEngine;
using Photon.Pun;

public class MultiplayerSpawnManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerNetwork";

    public Vector2 spawnAreaMin = new Vector2(-3, -3);
    public Vector2 spawnAreaMax = new Vector2(3, 3);

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            var player = PhotonNetwork.Instantiate(playerPrefabName, spawnOffset, Quaternion.identity);
            CameraFollow.Instance.objToFollow = player;
            Debug.Log("[MultiplayerSpawnManager] Spawned player: " + player.name + " | IsMine: " + player.GetComponent<PhotonView>().IsMine);
        }
        else
        {
            Debug.LogWarning("[MultiplayerSpawnManager] Not in a Photon room. Cannot spawn player.");
        }
    }
}