using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    [Header("Room UI (optional)")]
    public GameObject roomUIPanel;
    public TMP_InputField roomNameInput;

    [Header("Settings")]
    public int maxPlayersPerRoom = 4;

    private string roomPrefix;
    private string targetScene;
    private bool autoCreateRoomInNexus = false;

    public static bool skipAutoCreateRoom = false;

    void Start()
    {
        Debug.Log("🧠 Scene Start() - Photon InRoom: " + PhotonNetwork.InRoom);
        PhotonNetwork.AutomaticallySyncScene = true;

        string currentScene = SceneManager.GetActiveScene().name;
        switch (currentScene)
        {
            case "Nexus":
                roomPrefix = "Nexus_";
                targetScene = "Nexus";
                autoCreateRoomInNexus = true;
                break;

            case "Enter_Dungeon":
                roomPrefix = "Dungeon_";
                targetScene = "Dungeon";
                autoCreateRoomInNexus = false; // Rất quan trọng
                if (roomUIPanel) roomUIPanel.SetActive(true);
                break;
        }

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
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
        string roomName = roomNameInput?.text.Trim();
        if (string.IsNullOrEmpty(roomName)) roomName = roomPrefix + Random.Range(1000, 9999);

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
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

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
                cachedRoomList.Remove(room.Name);
            else
                cachedRoomList[room.Name] = room;
        }
    }


}
