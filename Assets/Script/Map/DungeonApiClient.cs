using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using Photon.Pun;
using Unity.VisualScripting;
[System.Serializable]
public class SkillCardSaveData
{
    public string active;
    public List<string> passive = new();
}
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
        public string currentCard;
        public string currentWeapon;
        public int enemyKills;
        public int deathCount;

    }
    

    private const string apiBase = "http://localhost:5058/api/dungeon";
    private const string apiBaseProgress = "http://localhost:5058/api/progress";
    private const string apiBaseTeammate = "http://localhost:5058/api/teammate";
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

    public IEnumerator DeleteOwnerProgress(int ownerId)
    {
        string url = apiBaseProgress + "/delete-owner-progress?ownerId=" + ownerId;

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.responseCode == 404)
            {
                Debug.LogWarning("⚠️ No save found to delete (already deleted or never created).");
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ OwnerProgress deleted successfully.");
            }
            else
            {
                Debug.LogError($"❌ Delete failed: {request.responseCode} - {request.error}");
                Debug.LogError(request.downloadHandler.text);
            }

        }
    }

    public IEnumerator LoadDungeonProgress(int ownerId, Action<int> onLoaded = null)
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
                        onLoaded?.Invoke(dto.ownerProgressId);
                    }));
                }
                //else
                //{
                //    onLoaded?.Invoke();
                //}
            }
            else
            {
                Debug.LogError("❌ Load failed: " + request.error);
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
    public IEnumerator SaveTeammateProgress(int userId, int ownerProgressId, PlayerProgressDTO dto)
    {
        if (ownerProgressId == -1) yield break;
        string url = $"{apiBaseTeammate}/save-teammate-progress?userId={userId}&ownerProgressId={ownerProgressId}";
        string json = JsonUtility.ToJson(dto);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ SaveTeammateProgress: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ SaveTeammateProgress Error: {request.error}");
            }
        }
    }

    public IEnumerator LoadTeammateProgress(int ownerId, int userId, Action<PlayerProgressDTO> callback)
    {
        string url = $"{apiBaseTeammate}/load-teammate-progress?ownerId={ownerId}&userId={userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Loaded teammate progress");
                var dto = JsonUtility.FromJson<PlayerProgressDTO>(request.downloadHandler.text);
                callback?.Invoke(dto);
            }
            else
            {
                Debug.Log($"❌ LoadTeammateProgress Error: {request.error}");
            }
        }
    }
    private string SerializeSkillCards(CharacterHandler handler)
    {
        SkillCardSaveData data = new SkillCardSaveData
        {
            active = handler.activeSkill != null ? handler.activeSkill.name.Replace("(Clone)", "").Trim() : ""
        };

        foreach (var passive in handler.GetPassiveSkills())
        {
            string clean = passive.name.Replace("(Clone)", "").Trim();
            data.passive.Add(clean);
        }

        return JsonUtility.ToJson(data);
    }


    /// <summary>
    /// Don't call this on teammate client
    /// </summary>
    /// <param name="playerTransform"></param>
    /// <param name="otherPlayer"></param>
    /// <returns></returns>
    public IEnumerator SaveProgressAfterSpawn(Transform playerTransform, List<int> otherPlayer = null,Action<int> progressIdCallback=null)
    {

        var handler = playerTransform.GetComponent<CharacterHandler>();
        if (handler == null )
        {
            Debug.LogError("❌ Missing CharacterHandler or PlayerProfile.");
            yield break;
        }

        int userId = PlayerProfileFetcher.CurrentProfile.userId;

        Debug.Log($"🟢 Saving PlayerProgress for userId = {userId}");
        var hptosave = handler.currentHealth == 0? 1: handler.currentHealth;
        var dto = new PlayerProgressDTO
        {

            currentHp = hptosave,
            currentMana = handler.currentMana,
            currentClass = handler.characterData.name,
            currentWeapon = handler.currentWeapon?.prefabName ?? "",
            currentCard = SerializeSkillCards(handler),
            enemyKills = PlayerStatTracker.Instance?.enemyKillCount ?? 0,
            deathCount = PlayerStatTracker.Instance?.deathCount ?? 0
        };

        yield return StartCoroutine(SaveOwnerProgress(
            userId, dto, (progressId) =>
            {

                if (progressId > 0)
                {
                    progressIdCallback?.Invoke(progressId);
                    Debug.Log($"✅ SaveOwnerProgress returned ProgressId = {progressId}");

                    string layout = DungeonGenerator.Instance.SaveLayout();
                    int stageLevel = DungeonGenerator.Instance.stageLevel;
                    if (string.IsNullOrEmpty(layout) || layout.Contains("\"entries\":[]"))
                    {
                        Debug.LogWarning("⚠️ Dungeon layout is empty. Skip saving dungeon.");
                        return;
                    }

                    DungeonProgressDTO dungeon = new DungeonApiClient.DungeonProgressDTO
                    {
                        ownerProgressId = progressId,
                        dungeonLayout = layout,
                        stageLevel = stageLevel
                    };

                    StartCoroutine(SaveDungeon(dungeon));
                    if (otherPlayer != null)
                    {
                        foreach (var tmId in otherPlayer)
                        {
                           if(tmId == -1) { continue; }
                           var tmdto = PlayerManager.Instance.GetPlayerProgress(tmId);
                            StartCoroutine(SaveTeammateProgress(tmId,progressId,tmdto));
                        }
                    }

                }

            }
        ));
        
    }

}
