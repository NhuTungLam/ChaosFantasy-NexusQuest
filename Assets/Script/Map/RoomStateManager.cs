using UnityEngine;
using System.Collections.Generic;
using DungeonSystem;

public class RoomStateManager : MonoBehaviour
{
    [Header("Optional: API Client")]
    public DungeonApiClient apiClient; // Gán nếu muốn gửi về server

    // Gọi hàm này để lưu trạng thái room hiện tại
    public void SaveRoom()
    {
        RoomState currentState = RoomStateBuilder.BuildRoomState();

        Debug.Log("[Room Save] Room ID: " + currentState.currentRoomId);
        Debug.Log("[Room Save] Player count: " + currentState.players.Count);
        Debug.Log("[Room Save] Enemy count: " + currentState.enemies.Count);

        string json = JsonUtility.ToJson(currentState);
        Debug.Log("[Room Save] JSON: " + json);
    }

    // Gọi hàm này để gửi lên API
    public void SaveRoomToServer()
    {
        RoomState currentState = RoomStateBuilder.BuildRoomState();

        List<string> playerIds = new List<string>();
        foreach (var p in currentState.players)
        {
            playerIds.Add(p.userId);
        }

        string roomStateJson = JsonUtility.ToJson(currentState); // serialize RoomState trước khi gửi

        if (apiClient != null)
        {
            StartCoroutine(apiClient.SaveProgress(
                currentState.currentRoomId,
                Photon.Pun.PhotonNetwork.LocalPlayer.UserId,
                playerIds,
                roomStateJson
            ));
        }
        else
        {
            Debug.LogWarning("[Room Save] API Client not assigned.");
        }
    }

    // Debug test khi bắt đầu game
    void Start()
    {
        // SaveRoom();
        // SaveRoomToServer();
    }
}
