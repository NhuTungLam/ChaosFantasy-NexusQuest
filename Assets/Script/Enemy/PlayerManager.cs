using Newtonsoft.Json.Converters;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPun
{
    public static PlayerManager Instance;
    public int ownerProgressId = -1;
    public int ownerViewId = -1;
    [PunRPC]
    public void RPC_OwnerProgressId(int ownerProcessId,int ViewId)
    {
        ownerProgressId = ownerProcessId;
        ownerViewId= ViewId;
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
                    view.transform.GetComponent<Rigidbody2D>().isKinematic = true;
                    tmView = UIStatTeammateManager.Assign();
                    view.transform.GetComponent<CharacterHandler>().AssignTeammateView(tmView);
                }
                playerList.Add(viewID, (userID, view.transform, tmView, false));
            }
        }

        if (userID == -1) return;

        if (!RoomSessionManager.Instance.IsRoomOwner() && PhotonView.Find(viewID).IsMine)
        {
            StartCoroutine(DelayLoadTeammateProgress());
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
    private IEnumerator DelayLoadTeammateProgress()
    {
        yield return new WaitForSeconds(1.5f);

        int ownerId = GetOwnerPlayerId();
        int myId = PlayerProfileFetcher.CurrentProfile?.userId ?? -1;
        //Debug.LogError($"[DELAYED] Loading teammate progress: ownerId={ownerId}, userId={myId}");
        if (ownerId > 0 && myId > 0)
        {
            yield return StartCoroutine(DungeonApiClient.Instance.LoadTeammateProgress(ownerId, myId, (dto) =>
            {
                MessageBoard.Show($"Loading {dto.currentClass}");
                var view = GetMyPlayer()?.GetComponent<PhotonView>();
                if (view != null)
                {
                    var handler = view.GetComponent<CharacterHandler>();
                    handler.ApplyLoadSave(dto);

                    // Gửi visual cho người khác
                    //if (PhotonNetwork.IsConnectedAndReady)
                    //{
                    //    view.RPC("RPC_LoadTeammateVisual", RpcTarget.Others, dto.currentClass, dto.currentWeapon);
                    //}
                }
            }));
        }
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
                    currentCard = SerializeSkillCards(handler)
                };

            }
        }

        return null;
    }
    private string SerializeSkillCards(CharacterHandler handler)
    {
        SkillCardSaveData data = new SkillCardSaveData
        {
            active = handler.activeSkill != null ? handler.activeSkill.name.Replace("(Clone)", "").Trim() : ""
        };

        foreach (var skill in handler.GetPassiveSkills())
        {
            data.passive.Add(skill.name.Replace("(Clone)", "").Trim());
        }

        return JsonUtility.ToJson(data);
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
        //foreach (var pair in playerList)
        //{
        //    Debug.LogError(pair.Key);
        //}
        //Debug.LogError(ownerViewId);
        if (playerList.TryGetValue(ownerViewId,out var group))
        {
            return group.userId;
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