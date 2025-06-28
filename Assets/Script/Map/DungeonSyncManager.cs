using Photon.Pun;
using System.Collections;
using UnityEngine;

public class DungeonSyncManager : MonoBehaviourPunCallbacks
{
    public static DungeonSyncManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    private IEnumerator Start()
    {
        // Đợi cho đến khi thật sự đã join room
        while (!PhotonNetwork.InRoom)
            yield return null;

        Debug.Log("🟢 DungeonSyncManager manually detected InRoom → init sync");

        if (RoomSessionManager.Instance == null)
        {
            Debug.LogWarning("⚠️ RoomSessionManager not found!");
            yield break;
        }

        if (RoomSessionManager.Instance.IsRoomOwner())
        {
            string savedLayout = DungeonRestorerManager.Instance?.dungeoninfo?.dungeonLayout;
            var stageLevel = DungeonRestorerManager.Instance?.dungeoninfo?.stageLevel;
            if (!string.IsNullOrEmpty(savedLayout))
            {

                Debug.Log("🟢 Room Owner loading saved layout...");
                DungeonGenerator.Instance.LoadLayout(savedLayout,stageLevel!=null? (int)stageLevel :1);
                photonView.RPC("RPC_SpawnRoomPrefab", RpcTarget.Others, savedLayout);
            }
            else
            {
                Debug.Log("🟢 Room Owner generating new dungeon...");
                DungeonGenerator.Instance.GenerateDungeon();
                photonView.RPC("RPC_SpawnRoomPrefab", RpcTarget.Others, DungeonGenerator.Instance.SaveLayout());
            }
        }
        else
        {
            Debug.Log("🟡 Teammate requesting layout from MasterClient...");
            photonView.RPC("RPC_RequestDungeonLayout", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
            
        }
    }
    private Room FindRoomAtPosition(Vector3 position)
    {
        Room[] rooms = GameObject.FindObjectsOfType<Room>();
        foreach (var room in rooms)
        {
            if (Vector3.Distance(room.transform.position, position) < 0.1f)
                return room;
        }
        return null;
    }
    [PunRPC]
    public void RPC_OpenDoorsAndSpawnChest(Vector3 roomPosition)
    {
        var room = FindRoomAtPosition(roomPosition);
        if (room != null)
        {
            room.OpenDoors();
            if (!room.chestSpawned && room.chestData != null && room.chestSpawnPoint != null)
            {
                room.SpawnChest();
                room.chestSpawned = true;
                if (room.roomType == RoomType.Boss)
                {
                    room.SpawnPortal();
                }
            }
        }
    }

    [PunRPC]
    public void RPC_SpawnRoomPrefab(string layout)
    {
        Debug.Log("🟣 RPC_SpawnRoomPrefab received → loading layout");
        DungeonGenerator.Instance.LoadLayout(layout,1);
    }

    [PunRPC]
    public void RPC_RequestDungeonLayout(int requesterActorNumber)
    {
        if (!RoomSessionManager.Instance.IsRoomOwner()) return;

        Photon.Realtime.Player target = PhotonNetwork.CurrentRoom.GetPlayer(requesterActorNumber);
        if (target != null)
        {
            Debug.Log($"📤 Sending layout to player {target.NickName} ({target.ActorNumber})");
            photonView.RPC("RPC_SpawnRoomPrefab", target, DungeonGenerator.Instance.SaveLayout());
        }
    }
}
