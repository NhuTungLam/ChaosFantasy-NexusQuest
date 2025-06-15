using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSyncManager : MonoBehaviourPunCallbacks
{
    private int roomsSpawned = 0;

    void Start()
    {
        if (RoomSessionManager.Instance != null && RoomSessionManager.Instance.IsRoomOwner())
        {
            DungeonGenerator.Instance.GenerateDungeon();
            
            photonView.RPC("RPC_SpawnRoomPrefab", RpcTarget.Others, DungeonGenerator.Instance.SaveLayout());
           
        }

    }
    public override void OnJoinedRoom()
    {
        if (!RoomSessionManager.Instance.IsRoomOwner())
        {
            // Ask the master client for the layout
            photonView.RPC("RPC_RequestDungeonLayout", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
    [PunRPC]
    public void RPC_SpawnRoomPrefab(string layout)
    {
        DungeonGenerator.Instance.LoadLayout(layout);
    }
    [PunRPC]
    public void RPC_RequestDungeonLayout(int requesterActorNumber)
    {
        if (!RoomSessionManager.Instance.IsRoomOwner()) return;

        Photon.Realtime.Player target = PhotonNetwork.CurrentRoom.GetPlayer(requesterActorNumber);
        photonView.RPC("RPC_SpawnRoomPrefab", target, DungeonGenerator.Instance.SaveLayout()); // send directly to requesting player
    }
}
