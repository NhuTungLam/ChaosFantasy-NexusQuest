using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomSessionManager : MonoBehaviourPunCallbacks
{
    public static RoomSessionManager Instance { get; private set; }

    private string roomOwnerId;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("✅ [RoomSessionManager] Awake and set as Singleton.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("✅ [RoomSessionManager] OnJoinedRoom() called.");

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("roomOwner", out object owner))
        {
            roomOwnerId = (string)owner;
            Debug.Log($"🏠 [RoomSessionManager] RoomOwner is {roomOwnerId}");
        }
        else
        {
            Debug.LogWarning("❗ [RoomSessionManager] roomOwner NOT found in CustomRoomProperties.");
        }
    }

    public bool IsRoomOwner()
    {
        bool result = PhotonNetwork.LocalPlayer.UserId == roomOwnerId;
        return result;
    }

    public string GetRoomOwner()
    {
        return roomOwnerId;
    }
}
