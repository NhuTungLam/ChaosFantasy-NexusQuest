using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

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

        Debug.Log("[RoomLoader] Attempting to resume room: " + roomIdToLoad);

        PhotonNetwork.JoinRoom(roomIdToLoad);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode == ErrorCode.GameDoesNotExist && !string.IsNullOrEmpty(roomIdToLoad))
        {
            Debug.Log("[RoomLoader] Room does not exist. Creating new room with ID: " + roomIdToLoad);

            RoomOptions options = new RoomOptions
            {
                MaxPlayers = 4,
                CustomRoomProperties = new Hashtable
                {
                    { "roomOwner", PhotonNetwork.LocalPlayer.UserId }
                },
                CustomRoomPropertiesForLobby = new string[] { "roomOwner" }
            };

            PhotonNetwork.CreateRoom(roomIdToLoad, options, TypedLobby.Default);
        }
        else
        {
            Debug.LogWarning($"[RoomLoader] Join room failed ({returnCode}): {message}");
            StartNewGame();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[RoomLoader] Rejoined or created room. Restoring RoomState...");

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

        optPanel?.SetActive(false);
    }

    void Start()
    {
        ShowLoadOptions();
    }
}
