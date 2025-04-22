using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomNameInput;
    public string dungeonSceneName = "Dungeon";

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("[PhotonRoomManager] Đang kết nối tới Photon...");
        }
    }

    public void CreateRoom()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room_" + Random.Range(1000, 9999);
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom()
    {
        string roomName = roomNameInput.text;
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
        string roomName = "QuickRoom_" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonRoomManager] Đã kết nối thành công đến Photon.");
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PhotonRoomManager] Đã vào phòng: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel(dungeonSceneName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("[PhotonRoomManager] Tạo phòng thất bại: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("[PhotonRoomManager] Vào phòng thất bại: " + message);
    }
}