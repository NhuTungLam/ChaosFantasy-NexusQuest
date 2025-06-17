using Photon.Pun;
using System.Collections;
using UnityEngine;

public class DungeonSyncManager : MonoBehaviourPunCallbacks
{
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

            if (!string.IsNullOrEmpty(savedLayout))
            {
                Debug.Log("🟢 Room Owner loading saved layout...");
                DungeonGenerator.Instance.LoadLayout(savedLayout);
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


    [PunRPC]
    public void RPC_SpawnRoomPrefab(string layout)
    {
        Debug.Log("🟣 RPC_SpawnRoomPrefab received → loading layout");
        DungeonGenerator.Instance.LoadLayout(layout);
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
