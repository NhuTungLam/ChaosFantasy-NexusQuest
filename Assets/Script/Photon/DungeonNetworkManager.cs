using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DungeonNetworkManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerNetwork";
    public Vector2 spawnAreaMin = new Vector2(-3, -3);
    public Vector2 spawnAreaMax = new Vector2(3, 3);

    private bool hasSpawned = false;

    public override void OnJoinedRoom()
    {
        Debug.Log("?? DungeonNetworkManager ? OnJoinedRoom");

        if (RoomSessionManager.Instance == null)
        {
            Debug.LogWarning("?? RoomSessionManager not ready.");
            return;
        }

        if (RoomSessionManager.Instance.IsRoomOwner())
        {
            string savedLayout = DungeonRestorerManager.Instance?.dungeoninfo?.dungeonLayout;

            if (!string.IsNullOrEmpty(savedLayout))
            {
                Debug.Log("?? Room Owner loading saved layout...");
                DungeonGenerator.Instance.LoadLayout(savedLayout);
                photonView.RPC("RPC_SpawnRoomPrefab", RpcTarget.Others, savedLayout);
            }
            else
            {
                Debug.Log("?? Room Owner generating new dungeon...");
                DungeonGenerator.Instance.GenerateDungeon();
                photonView.RPC("RPC_SpawnRoomPrefab", RpcTarget.Others, DungeonGenerator.Instance.SaveLayout());
            }

            TrySpawn(); // ? Spawn luôn sau khi host x? lý map
        }
        else
        {
            Debug.Log("?? Teammate requesting layout...");
            photonView.RPC("RPC_RequestDungeonLayout", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    public void RPC_SpawnRoomPrefab(string layout)
    {
        Debug.Log("?? RPC_SpawnRoomPrefab received ? load layout + spawn");

        DungeonGenerator.Instance.LoadLayout(layout);
        TrySpawn(); // ? Teammate spawn sau khi nh?n layout
    }

    [PunRPC]
    public void RPC_RequestDungeonLayout(int requesterActorNumber)
    {
        if (!RoomSessionManager.Instance.IsRoomOwner()) return;

        Photon.Realtime.Player target = PhotonNetwork.CurrentRoom.GetPlayer(requesterActorNumber);
        if (target != null)
        {
            Debug.Log($"?? Sending layout to player {target.NickName}");
            photonView.RPC("RPC_SpawnRoomPrefab", target, DungeonGenerator.Instance.SaveLayout());
        }
    }

    private void TrySpawn()
    {
        if (hasSpawned)
        {
            Debug.Log("?? Already spawned.");
            return;
        }

        // Get character data
        CharacterData data = null;
        var playerInfo = DungeonRestorerManager.Instance?.playerinfo;
        if (playerInfo != null)
        {
            data = Resources.Load<CharacterData>("Characters/" + playerInfo.currentClass);
        }
        if (data == null)
        {
            Debug.LogWarning("?? Fallback to Archer");
            data = Resources.Load<CharacterData>("Characters/Archer");
        }

        // Spawn position
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            0f
        );

        // Instantiate player
        GameObject playerInstance = PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPos,
            Quaternion.identity,
            0,
            new object[] { data.name }
        );

        // Apply loaded stats
        var handler = playerInstance.GetComponent<CharacterHandler>();
        handler.currentHealth = playerInfo?.currentHp ?? data.MaxHealth;
        handler.currentMana = playerInfo?.currentMana ?? data.MaxMana;

        if (!string.IsNullOrEmpty(playerInfo?.currentWeapon))
        {
            var weaponData = Resources.Load<WeaponData>("Weapons/" + playerInfo.currentWeapon);
            if (weaponData != null)
                handler.EquipWeapon(weaponData);
        }

        // Sync with PlayerManager
        int viewID = playerInstance.GetComponent<PhotonView>().ViewID;
        PlayerManager.Instance.photonView.RPC(
            "RPC_AddPlayerToList",
            RpcTarget.AllBuffered,
            viewID
        );

        hasSpawned = true;
    }
}
