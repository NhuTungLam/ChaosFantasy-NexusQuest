using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPun
{
    public static PlayerManager Instance;

    // Dictionary: Key = PhotonView.ViewID, Value = Player Transform
    public Dictionary<int, Transform> playerList = new();

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
            StartCoroutine(DungeonApiClient.Instance.LoadTeammateProgress(GetOwnerPlayerId(), userID, (dto) =>
            {
                playerList[userID].GetComponent<CharacterHandler>().ApplyLoadSave(dto);
            }));
        }
    }


    public bool ContainsPlayer(int viewID)
    {
        return playerList.ContainsKey(viewID);
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
                currentWeapon = handler.currentWeapon?.weaponData?.name ?? "",
                currentCards = "" // TODO: serialize skill cards if needed
            };
        }

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

