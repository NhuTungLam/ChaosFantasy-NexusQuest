using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject roomItemPrefab;
    [SerializeField] private Transform roomListContainer;
    public static PhotonRoomManager Instance;
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
    [Header("Room UI (optional)")]
    private TMP_InputField roomNameInput = null;
    private Button joinBtn, createBtn, autoCreateBtn, continueBtn;
    [Header("Settings")]
    public int maxPlayersPerRoom = 4;

    private string roomPrefix;
    private string targetScene;
    private bool autoCreateRoomInNexus = false;

    public static bool skipAutoCreateRoom = false;

    void Start()
    {
        roomPrefix = "Nexus_";
        targetScene = "Nexus";
        autoCreateRoomInNexus = true;
        Debug.Log("🧠 Scene Start() - Photon InRoom: " + PhotonNetwork.InRoom);
        PhotonNetwork.AutomaticallySyncScene = true;
        
        SceneManager.activeSceneChanged += (prevScene,currentScene) =>
        {
            roomNameInput = null;
            createBtn = null;
            autoCreateBtn = null;
            continueBtn = null;
            joinBtn = null;
            switch (currentScene.name)
            {
                case "Nexus":
                    roomPrefix = "Nexus_";
                    targetScene = "Nexus";
                    autoCreateRoomInNexus = true;
                    break;

                case "Enter_Dungeon":
                    roomPrefix = "Dungeon_";
                    targetScene = "Dungeon";
                    autoCreateRoomInNexus = false;
                    roomNameInput = GameObject.Find("Canvas/RoomJoinPanel/room_id").GetComponent<TMP_InputField>();
                    createBtn = GameObject.Find("Canvas/RoomJoinPanel/create").GetComponent<Button>();
                    joinBtn = GameObject.Find("Canvas/RoomJoinPanel/join").GetComponent<Button>();
                    autoCreateBtn = GameObject.Find("Canvas/RoomJoinPanel/auto_create").GetComponent<Button>();
                    continueBtn = GameObject.Find("Canvas/RoomJoinPanel/continue").GetComponent<Button>();
                    roomListContainer = GameObject.Find("Canvas/RoomJoinPanel/room_list/Viewport/Content").transform;
                    roomItemPrefab = Resources.Load<GameObject>("room_name");

                    createBtn.onClick.AddListener(CreateRoom);
                    joinBtn.onClick.AddListener(JoinRoom);
                    continueBtn.onClick.AddListener(LoadSave);
                    autoCreateBtn.onClick.AddListener(QuickStart);

                    UpdateRoomListUI();

                    break;
            }
        };

        if (!PhotonNetwork.IsConnected)
        {
            if (PlayerProfileFetcher.CurrentProfile != null)
            {
                PhotonNetwork.NickName = PlayerProfileFetcher.CurrentProfile.username;
            }
            else
            {
                PhotonNetwork.NickName = "Guest_" + Random.Range(1000, 9999); 
            }
            PhotonNetwork.GameVersion = "1.0";
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Connected to Photon Master Server.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Joined Photon Lobby.");

        if (autoCreateRoomInNexus && !skipAutoCreateRoom)
        {
            Debug.Log("🔁 Auto-creating Nexus room after join lobby.");
            CreatePrivateRoom();
        }
        else
        {
            Debug.Log("ℹ️ Not auto-creating room.");
        }
    }

    private void CreatePrivateRoom()
    {
        string roomName = roomPrefix + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = false,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "roomOwner", PhotonNetwork.LocalPlayer.UserId }
        },
            CustomRoomPropertiesForLobby = new string[] { "roomOwner" }
        };

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        Debug.Log("🌐 Created private room: " + roomName);
    }

    public void CreateRoom()
    {
        if (PlayerProfileFetcher.CurrentProfile != null)
        {
            StartCoroutine(DungeonApiClient.Instance.DeleteOwnerProgress(PlayerProfileFetcher.CurrentProfile.userId));
            
        }
        string roomName = roomNameInput?.text.Trim();
        if (string.IsNullOrEmpty(roomName)) roomName = roomPrefix + Random.Range(1000, 9999);

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = true, // ⚠️ Quan trọng để phòng hiển thị
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "roomOwner", PhotonNetwork.LocalPlayer.UserId }
    },
            CustomRoomPropertiesForLobby = new string[] { "roomOwner" }
        };

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }


    public void JoinRoom()
    {
        string roomName = roomNameInput?.text.Trim();
        if (!string.IsNullOrEmpty(roomName))
        {
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public void QuickStart()
    {
        if (PlayerProfileFetcher.CurrentProfile != null)
        {
            StartCoroutine(DungeonApiClient.Instance.DeleteOwnerProgress(PlayerProfileFetcher.CurrentProfile.userId));

        }
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("⚠️ Quick Start failed, fallback to create room.");
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {

        Debug.Log("✅ Joined room: " + PhotonNetwork.CurrentRoom.Name);

        if (!string.IsNullOrEmpty(targetScene) &&
            SceneManager.GetActiveScene().name != targetScene)
        {
            Debug.Log("✅ Joined room: " + PhotonNetwork.CurrentRoom.Name);
            Debug.Log("🔁 Loading scene: " + targetScene);
            // Sửa SceneManager → PhotonNetwork
            PhotonNetwork.LoadLevel(targetScene);  // ✅ Rất quan trọng
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[JoinRoom] Failed to join room: {message} (code {returnCode})");

        Debug.Log("Available rooms in lobby:");
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            Debug.Log($" - {room.Name}, Owner: {room.CustomProperties["roomOwner"]}");
        }
    }
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public void LoadSave()
    {
        int userId = PlayerProfileFetcher.CurrentProfile !=null? PlayerProfileFetcher.CurrentProfile.userId: -1;
        if(userId == -1)
        {
            MessageBoard.Show("Save/Load feature is not available in guest mode");
        }
        StartCoroutine(DungeonApiClient.Instance.LoadDungeonProgress(userId, (i) =>
        {
            if (DungeonRestorerManager.Instance.dungeoninfo != null)
            {
                MessageBoard.Show("Loading save...");

                // Tạo room trước khi vào dungeon
                RoomOptions options = new RoomOptions
                {
                    MaxPlayers = 4, 
                    IsVisible = true,
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "roomOwner", PhotonNetwork.LocalPlayer.UserId }
                },
                    CustomRoomPropertiesForLobby = new string[] { "roomOwner" }
                };

                string roomName = "Dungeon_Loaded_" + UnityEngine.Random.Range(1000, 9999);
                PhotonNetwork.CreateRoom(roomName, options); // khi OnJoinedRoom → LoadLevel("Dungeon")
            }
            else
            {
                MessageBoard.Show("No save found!");
            }
        }));
    }

    

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList || room.PlayerCount == 0)
                cachedRoomList.Remove(room.Name);
            else
                cachedRoomList[room.Name] = room;
        }

        if (SceneManager.GetActiveScene().name == "Enter_Dungeon")
            UpdateRoomListUI();
    }

    public void UpdateRoomListUI()
    {
        // Clear cũ
        foreach (Transform child in roomListContainer)
            Destroy(child.gameObject);

        // Hiển thị phòng có người chơi
        foreach (var room in cachedRoomList.Values)
        {
            if (room.PlayerCount == 0) continue;

            GameObject item = Instantiate(roomItemPrefab, roomListContainer);

            // Set text
            var text = item.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";

            // Set join button
            var btn = item.GetComponent<Button>();
            if (btn != null)
            {
                string roomName = room.Name; // tránh closure bug
                btn.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomName));
            }
        }
    }

}
