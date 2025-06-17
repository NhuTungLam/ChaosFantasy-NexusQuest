using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using Photon.Pun;

public class DungeonApiClient : MonoBehaviour
{
    public static DungeonApiClient Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [System.Serializable]
    public class ProgressIdResult
    {
        public int progressId;
    }
    [System.Serializable]
    public class DungeonProgressDTO
    {
        public int ownerProgressId;
        public string dungeonLayout;
        public int stageLevel;
    }
    [System.Serializable]
    public class PlayerProgressDTO 
    {
        public float currentHp;
        public float currentMana;
        public string currentClass;
        public string currentCards;
        public string currentWeapon;

    }
    private const string apiBase = "http://localhost:5058/api/dungeon";
    private const string apiBaseProgress = "http://localhost:5058/api/progress";
    public IEnumerator SaveOwnerProgress(int ownerId, PlayerProgressDTO progressDTO, Action<int> onProgressIdReceived)
    {
        string json = JsonUtility.ToJson(progressDTO);
        Debug.Log("[API] Sending JSON to SaveOwnerProgress: " + json);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiBaseProgress + "/owner/save?ownerId=" + ownerId, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Success: " + request.downloadHandler.text);

                try
                {
                    int progressId = JsonUtility.FromJson<ProgressIdResult>(request.downloadHandler.text).progressId;
                    onProgressIdReceived?.Invoke(progressId);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse progressId.");
                    onProgressIdReceived?.Invoke(-1);
                }
            }
            else
            {
                Debug.LogError("[API] SaveOwnerProgress failed:");
                Debug.LogError("URL: " + request.url);
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response Code: " + request.responseCode);
                Debug.LogError("Response Text: " + request.downloadHandler.text);
                onProgressIdReceived?.Invoke(-1);
            }
        }
    }

    public IEnumerator SavePlayerProgress(PlayerProgressDTO playerProgress)
    {
        string json = JsonUtility.ToJson(playerProgress);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiBaseProgress + "/save-by-user", "POST"))
        {
            

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {

                Debug.Log("Success: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response Code: " + request.responseCode);
                Debug.LogError("Response Text: " + request.downloadHandler.text);

            }
        }

    }
    public IEnumerator SaveProgress(int ownerId, List<int> playerList, string roomStateJson, int stageLevel)
    {
        var ownerProgressDTO = PlayerManager.Instance.GetPlayerProgress(ownerId);

        yield return StartCoroutine(SaveOwnerProgress(ownerId, ownerProgressDTO, (ownerProgressId) =>
        {
            if (ownerProgressId <= 0)
            {
                Debug.LogError("[API] Failed to save owner progress or received invalid ProgressId.");
                return;
            }

            StartCoroutine(DelayedSaveDungeon(ownerProgressId, roomStateJson, stageLevel));
        }));
    }
    private IEnumerator DelayedSaveDungeon(int ownerProgressId, string roomStateJson, int stageLevel)
    {
        yield return new WaitForSeconds(0.1f);

        DungeonProgressDTO dungeonData = new DungeonProgressDTO
        {
            ownerProgressId = ownerProgressId,
            dungeonLayout = roomStateJson ?? "{}",
            stageLevel = stageLevel
        };

        yield return StartCoroutine(SaveDungeon(dungeonData));
    }


    public IEnumerator LoadDungeonProgress(int ownerId, Action onLoaded = null)
    {
        string url = apiBase + "/owner/load-progress?ownerId=" + ownerId;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                DungeonProgressDTO dto = JsonUtility.FromJson<DungeonProgressDTO>(request.downloadHandler.text);
                DungeonRestorerManager.Instance.dungeoninfo = dto;

                if (dto != null)
                {
                    StartCoroutine(LoadPlayerProgress(dto.ownerProgressId, (d) =>
                    {
                        DungeonRestorerManager.Instance.playerinfo = d;
                        onLoaded?.Invoke();
                    }));
                }
                else
                {
                    onLoaded?.Invoke();
                }
            }
            else
            {
                Debug.LogError("❌ Load failed: " + request.error);
                onLoaded?.Invoke(); 
            }
        }
    }


    public IEnumerator LoadPlayerProgress(int progressId, Action<PlayerProgressDTO> callback)
    {
        string url = apiBaseProgress + "/load?progressId=" + progressId;

        using (UnityWebRequest request = UnityWebRequest.Get(url)) // ✅ Dùng GET
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Loaded player progress.");
                PlayerProgressDTO dto = JsonUtility.FromJson<PlayerProgressDTO>(request.downloadHandler.text);
                Debug.LogWarning(request.downloadHandler.text);

                callback?.Invoke(dto);
            }
            else
            {
                Debug.LogError("❌ Failed to load progress: " + request.error);
            }
        }
    }

    public IEnumerator SaveDungeon(DungeonProgressDTO dungeon)
    {
        string json = JsonUtility.ToJson(dungeon);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        Debug.Log(dungeon.dungeonLayout);
        using (UnityWebRequest request = new UnityWebRequest(apiBase + "/save-progress", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Success: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
    public IEnumerator SaveProgressAfterSpawn(Transform playerTransform)
    {
        var handler = playerTransform.GetComponent<CharacterHandler>();
        if (handler == null || handler.profile == null)
        {
            Debug.LogError("❌ Missing CharacterHandler or PlayerProfile.");
            yield break;
        }

        int userId = PlayerProfileFetcher.CurrentProfile.userId;

        Debug.Log($"🟢 Saving PlayerProgress for userId = {userId}");
        var hptosave = handler.currentHealth == 0? 1: handler.currentHealth;
        var dto = new DungeonApiClient.PlayerProgressDTO
        {

            currentHp = hptosave,
            currentMana = handler.currentMana,
            currentClass = handler.characterData.name,
            currentWeapon = handler.currentWeapon?.weaponData?.name ?? "",
            currentCards = "" // optional
        };


        // 🔒 Save dungeon if this player is room owner
        var roomOwnerId = PhotonNetwork.CurrentRoom.CustomProperties["roomOwner"]?.ToString();
        if (PhotonNetwork.LocalPlayer.UserId == roomOwnerId)
        {
            Debug.Log("🟢 This player is the roomOwner → Save dungeon too");

            yield return StartCoroutine(DungeonApiClient.Instance.SaveOwnerProgress(
                userId, dto, (progressId) =>
                {
                    if (progressId > 0)
                    {
                        Debug.Log($"✅ SaveOwnerProgress returned ProgressId = {progressId}");

                        string layout = DungeonGenerator.Instance.SaveLayout();
                        if (string.IsNullOrEmpty(layout) || layout.Contains("\"entries\":[]"))
                        {
                            Debug.LogWarning("⚠️ Dungeon layout is empty. Skip saving dungeon.");
                            return;
                        }

                        DungeonApiClient.DungeonProgressDTO dungeon = new DungeonApiClient.DungeonProgressDTO
                        {
                            ownerProgressId = progressId,
                            dungeonLayout = layout,
                            stageLevel = 1
                        };
                        StartCoroutine(DungeonApiClient.Instance.SaveDungeon(dungeon));
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Failed to save OwnerProgress.");
                    }
                }
            ));
        }
    }

}
