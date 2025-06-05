using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class DungeonApiClient : MonoBehaviour
{
    [System.Serializable]
    public class DungeonProgressDTO
    {
        public string roomId;
        public string ownerId;
        public List<string> playerList;
        public string roomState; 
    }

    private const string apiBase = "http://localhost:5058/api/dungeon";

    public IEnumerator SaveProgress(string roomId, string ownerId, List<string> playerList, string roomStateJson)
    {
        DungeonProgressDTO data = new DungeonProgressDTO
        {
            roomId = roomId ?? "unknown-room",
            ownerId = ownerId ?? "unknown-owner",
            playerList = playerList ?? new List<string>(),
            roomState = roomStateJson ?? "{}"
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("[DEBUG] Sending JSON:\n" + json);

        var request = new UnityWebRequest(apiBase + "/save-progress", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[API] Save failed: {request.error}");
        }
        else
        {
            Debug.Log("[API] Dungeon progress saved successfully.");
        }
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
}
