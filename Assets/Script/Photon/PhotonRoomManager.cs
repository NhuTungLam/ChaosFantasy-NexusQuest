using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField roomNameInput;
    public string dungeonSceneName = "Dungeon"; 

    void Start()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogWarning("Room name is empty!");
            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogWarning("Room name is empty!");
            return;
        }

        PhotonNetwork.JoinRoom(roomNameInput.text);
    }

    public void QuickStart()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join random failed, creating new room...");
        string randomRoomName = "Room_" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(randomRoomName, options);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[PhotonRoomManager] Đã vào phòng: {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LoadLevel(dungeonSceneName); 
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Create room failed: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join room failed: " + message);
    }
}
