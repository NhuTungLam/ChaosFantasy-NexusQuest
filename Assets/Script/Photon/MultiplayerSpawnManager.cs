using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerSpawnManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerNetwork";
    public Vector2 spawnAreaMin = new Vector2(-3, -3);
    public Vector2 spawnAreaMax = new Vector2(3, 3);

    private bool hasSpawned = false;

    void Start()
    {

        Debug.Log("🟦 MultiplayerSpawnManager.Start() is alive.");

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            Debug.Log("🟦 Detected already in room → TrySpawn manually");
            TrySpawn();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("🟢 OnJoinedRoom (MultiplayerSpawnManager)");
        TrySpawn();
    }

    void TrySpawn()
    {
        if (hasSpawned)
        {
            Debug.Log("⚠️ Already spawned.");
            return;
        }

        if (CharacterSelector.Instance == null)
        {
            GameObject selector = new GameObject("CharacterSelector");
            selector.AddComponent<CharacterSelector>();
        }

        CharacterData data = CharacterSelector.LoadData();
        if (data == null)
        {
            Debug.LogWarning("⚠️ Fallback to 'Archer'");
            data = Resources.Load<CharacterData>("Characters/Archer");
            if (data == null)
            {
                Debug.LogError("❌ Không tìm thấy 'Archer' trong Resources.");
                return;
            }
        }

        string className = data.name;
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            0f
        );

        GameObject playerInstance = PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPos,
            Quaternion.identity,
            0,
            new object[] { className }
        );

        int viewID = playerInstance.GetComponent<PhotonView>().ViewID;

        PlayerManager.Instance.photonView.RPC(
            "RPC_AddPlayerToList",
            RpcTarget.AllBuffered,
            viewID
        );

        hasSpawned = true;

        // 🟢 Save player progress to backend
        StartCoroutine(DungeonApiClient.Instance.SaveProgressAfterSpawn(playerInstance.transform));
    }
}
