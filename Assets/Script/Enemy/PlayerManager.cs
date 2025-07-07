using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviourPun
{
    public static PlayerManager Instance;

    // Dictionary: Key = PhotonView.ViewID, Value = Player Transform
    public Dictionary<int, Transform> playerList = new();
    public List<CharacterHandler> allPlayers = new();

    public bool AreAllPlayersDead()
    {
        return allPlayers.All(p => p == null || p.currentHealth <= 0);
    }
    public void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Clean up null transforms
        var keysToRemove = new List<int>();
        foreach (var kvp in playerList)
        {
            if (kvp.Value == null)
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
        {
            playerList.Remove(key);
        }
    }

    

    [PunRPC]
    public void RPC_AddPlayerToList(int viewID,int userID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null && view.transform != null)
        {
            if (!playerList.ContainsKey(userID))
            {
                playerList.Add(userID, view.transform);
                Debug.Log("[PlayerManager] RPC synced player: " + view.transform.name);
            }
        }
        if (userID == -1) return;
        if (!RoomSessionManager.Instance.IsRoomOwner() && PhotonView.Find(viewID).IsMine)
        {
            int ownerId = GetOwnerPlayerId();
            int myId = PlayerProfileFetcher.CurrentProfile?.userId ?? -1;

            if (ownerId > 0 && myId > 0)
                StartCoroutine(DelayLoadTeammateProgress(ownerId, myId));
        }
        var handler = view.GetComponent<CharacterHandler>();
        if (handler != null && !allPlayers.Contains(handler))
        {
            allPlayers.Add(handler);
        }

    }
    [PunRPC]
    public void RPC_TeleportPlayer(int triggerPlayerId)
    {
        List<Transform> transforms = new List<Transform>();
        foreach (var pair in playerList)
        {
            if (pair.Key == triggerPlayerId)
            {
                continue;
            }
            transforms.Add(pair.Value);
        }
        Transform trigger = playerList[triggerPlayerId].transform;
        foreach (var t in transforms)
        {
            t.position = trigger.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }
    }
    [PunRPC]
    public void RPC_SelfTeleport(Vector2 position)
    {
        GetMyPlayer().position = position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }
    public bool ContainsPlayer(int viewID)
    {
        return playerList.ContainsKey(viewID);
    }
    private IEnumerator DelayLoadTeammateProgress(int ownerId, int myId)
    {
        // ⏳ Delay 1 second cho owner kịp SaveProgress
        yield return new WaitForSeconds(1.5f);

        Debug.Log($"[DELAYED] Loading teammate progress: ownerId={ownerId}, userId={myId}");

        yield return StartCoroutine(DungeonApiClient.Instance.LoadTeammateProgress(ownerId, myId, (dto) =>
        {
            var view = GetMyPlayer()?.GetComponent<PhotonView>();
            if (view != null)
                view.GetComponent<CharacterHandler>().ApplyLoadSave(dto);
        }));
    }

    public DungeonApiClient.PlayerProgressDTO GetPlayerProgress(int userId)
    {
        var player = playerList[userId];
        var handler = player.GetComponent<CharacterHandler>();
        {
            return new DungeonApiClient.PlayerProgressDTO
            {
                currentHp = handler.currentHealth,
                currentMana = handler.currentMana,
                currentClass = handler.characterData.name,
                currentWeapon = handler.currentWeapon?.prefabName ?? "",
                currentCards = "" // TODO: serialize skill cards if needed
            };
        }

    }
    public int? GetPlayerIdByTransform(Transform target)
    {
        foreach (var pair in playerList)
        {
            if (pair.Value == target)
                return pair.Key;
        }
        return null; // Không tìm th?y
    }

    public int GetOwnerPlayerId()
    {
        string roomOwnerId = PhotonNetwork.CurrentRoom.CustomProperties["roomOwner"]?.ToString();

        foreach (var kvp in playerList)
        {
            var view = kvp.Value.GetComponent<PhotonView>();
            if (view != null && view.Owner.UserId == roomOwnerId)
            {
                return kvp.Key;
            }
        }

        return -1; // not found
    }


    public Transform GetMyPlayer()
    {
        foreach (var player in playerList.Values)
        {
            var view = player.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
                return player;
        }

        return null;
    }
    //for enemy
    public Transform GetPlayer(Vector2 origin, float range = 5f)
    {
        Transform closestPlayer = null;
        float minDistance = Mathf.Infinity;

        foreach (var player in playerList.Values)
        {
            if (player == null) continue;

            float distance = Vector2.Distance(origin, player.position);
            if (distance <= range && distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public List<int> GetOtherPlayer()
    {
        List<int> otherPlayerIds = new();

        foreach (var kvp in playerList)
        {
            var view = kvp.Value.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
                continue;

            otherPlayerIds.Add(kvp.Key);
        }

        return otherPlayerIds;
    }
}

