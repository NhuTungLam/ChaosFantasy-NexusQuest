using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPun
{
    public static PlayerManager Instance;
    public int ownerProgressId = -1;
    [PunRPC]
    public void RPC_OwnerProgressId(int Id)
    {
        ownerProgressId =Id;
    }
    public void CheckDie(int viewId,bool down)
    {
        var p = playerList[viewId];
        p.isDown = down;
        playerList[viewId] = p;
        if (down)
        {
            bool allDown = true;
            foreach (var v in playerList)
            {
                if (!v.Value.isDown)
                {
                    allDown = false;
                    break;
                }
                
            }
            if (allDown == true)
            {
                photonView.RPC("RPC_Summary", RpcTarget.All);
            }
        }
    }
    [PunRPC]
    public void RPC_Summary()
    {
        GameManager.Instance.ShowSummaryPanel();
    }
    // Key = viewID, Value = (userId, playerTransform)
    public Dictionary<int, (int userId, Transform playerTransform, RectTransform teammateUI,bool isDown)> playerList = new();
   
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
            if (kvp.Value.playerTransform == null)
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
        {
            UIStatTeammateManager.UnAssign(playerList[key].teammateUI);
            playerList.Remove(key);
        }
    }

    [PunRPC]
    public void RPC_AddPlayerToList(int viewID, int userID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null && view.transform != null)
        {
            if (!playerList.ContainsKey(viewID))
            {
                //Debug.Log("[PlayerManager] RPC synced player: " + view.transform.name);
                RectTransform tmView = null;
                if (!PhotonView.Find(viewID).IsMine)
                {
                    tmView = UIStatTeammateManager.Assign();
                    view.transform.GetComponent<CharacterHandler>().AssignTeammateView(tmView);
                }
                playerList.Add(viewID, (userID, view.transform, tmView, false));
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
    }

    [PunRPC]
    public void RPC_SelfTeleport(Vector2 position)
    {
        var myPlayer = GetMyPlayer();
        if (myPlayer != null)
        {
            myPlayer.position = position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }
    }

    public bool ContainsPlayer(int viewID)
    {
        return playerList.ContainsKey(viewID);
    }

    private IEnumerator DelayLoadTeammateProgress(int ownerId, int myId)
    {
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
        foreach (var kvp in playerList)
        {
            if (kvp.Value.userId == userId)
            {
                var handler = kvp.Value.playerTransform.GetComponent<CharacterHandler>();
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

        return null;
    }

    public int? GetPlayerIdByTransform(Transform target)
    {
        foreach (var pair in playerList)
        {
            if (pair.Value.playerTransform == target)
                return pair.Value.userId;
        }
        return null;
    }

    public int GetOwnerPlayerId()
    {
        string roomOwnerId = PhotonNetwork.CurrentRoom.CustomProperties["roomOwner"]?.ToString();

        foreach (var kvp in playerList)
        {
            var view = kvp.Value.playerTransform.GetComponent<PhotonView>();
            if (view != null && view.Owner.UserId == roomOwnerId)
            {
                return kvp.Value.userId;
            }
        }

        return -1;
    }

    public Transform GetMyPlayer()
    {
        foreach (var pair in playerList.Values)
        {
            var view = pair.playerTransform.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
                return pair.playerTransform;
        }

        return null;
    }

    // For enemy AI to find nearby player
    public Transform GetPlayer(Vector2 origin, float range = 5f)
    {
        Transform closestPlayer = null;
        float minDistance = Mathf.Infinity;

        foreach (var player in playerList.Values)
        {
            if (player.playerTransform == null) continue;

            float distance = Vector2.Distance(origin, player.playerTransform.position);
            if (distance <= range && distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player.playerTransform;
            }
        }

        return closestPlayer;
    }

    public List<int> GetOtherPlayer()
    {
        List<int> otherPlayerIds = new();

        foreach (var kvp in playerList)
        {
            var view = kvp.Value.playerTransform.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
                continue;

            otherPlayerIds.Add(kvp.Value.userId);
        }

        return otherPlayerIds;
    }
}