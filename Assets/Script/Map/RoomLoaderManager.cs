using UnityEngine;
using Photon.Pun;

public class RoomLoaderManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public DungeonApiClient apiClient;
    public RoomStateRestorer stateRestorer;
    public GameObject roomUI;
    public GameObject optPanel;

    [HideInInspector]
    public string roomIdToLoad;

    public void ShowLoadOptions()
    {
        if (PlayerPrefs.HasKey("lastRoomId"))
        {
            roomIdToLoad = PlayerPrefs.GetString("lastRoomId");
            optPanel?.SetActive(true);
        }
        else
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        optPanel?.SetActive(false);
        roomUI?.SetActive(true);
        Debug.Log("[RoomLoader] Starting new game mode.");
    }

    public void ResumeSavedGame()
    {
        if (string.IsNullOrEmpty(roomIdToLoad))
        {
            Debug.LogWarning("[RoomLoader] No saved room ID");
            return;
        }
        PhotonNetwork.JoinRoom(roomIdToLoad);
        Debug.Log("[RoomLoader] Attempting to rejoin room: " + roomIdToLoad);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[RoomLoader] Rejoined room. Attempting to restore dungeon...");
        if (apiClient != null && stateRestorer != null && !string.IsNullOrEmpty(roomIdToLoad))
        {
            StartCoroutine(apiClient.LoadProgress(roomIdToLoad, (json) =>
            {
                Debug.Log("[RoomLoader] Loaded room JSON: " + json);
                stateRestorer.RestoreFromJson(json);
            }));
        }
        else
        {
            Debug.LogWarning("[RoomLoader] Missing references or empty roomId during OnJoinedRoom");
        }
    }

    void Start()
    {
        ShowLoadOptions();
    }
}
