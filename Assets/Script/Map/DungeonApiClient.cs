using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using Photon.Pun;

public class DungeonApiClient : MonoBehaviour
{
    public static DungeonApiClient Instance { get; private set; }

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
        public string roomState;
        public int stageLevel;
    }
    [System.Serializable]
    public class PlayerProgressDTO 
    {
        public float currentHP;
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
            roomState = roomStateJson ?? "{}",
            stageLevel = stageLevel
        };

        yield return StartCoroutine(SaveDungeon(dungeonData));
    }


    public IEnumerator LoadProgress(string roomId, System.Action<string> onLoaded)
    {
        string url = apiBase + $"/load-progress?roomId={roomId}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[API] Load failed: {request.error}");
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.Log("[API] Loaded dungeon progress: " + json);
            onLoaded?.Invoke(json);
        }
    }
    public IEnumerator SaveDungeon(DungeonProgressDTO dungeon)
    {
        string json = JsonUtility.ToJson(dungeon);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

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

        var dto = new DungeonApiClient.PlayerProgressDTO
        {
            currentHP = handler.currentHealth,
            currentMana = handler.currentMana,
            currentClass = handler.characterData.name,
            currentWeapon = handler.currentWeapon?.weaponData?.name ?? "",
            currentCards = "" // có thể mở rộng
        };

        // 🟢 Lưu tiến trình cơ bản cho player
        //yield return StartCoroutine(DungeonApiClient.Instance.SavePlayerProgress(dto));

        // 🔒 Nếu là chủ phòng, gọi SaveOwnerProgress để trigger SaveDungeon sau đó
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

                        // Save dungeon tiến trình kèm theo (roomState đã bỏ, stage = 1)
                        DungeonApiClient.DungeonProgressDTO dungeon = new DungeonApiClient.DungeonProgressDTO
                        {
                            ownerProgressId = progressId,
                            roomState = "{}", // không dùng
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
