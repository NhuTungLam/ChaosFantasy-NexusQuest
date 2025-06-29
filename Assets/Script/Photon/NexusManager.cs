using UnityEngine;
using Photon.Pun;

public class NexusManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerNetwork";
    public Transform spawnPoint;

    public override void OnJoinedRoom()
    {
        Debug.Log(" Joined Nexus room → Spawning player...");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log(" Player already spawned.");
            return;
        }

        CharacterData selectedClass = CharacterSelector.LoadData();
        if (selectedClass == null)
        {
            Debug.LogError(" No class selected.");
            return;
        }

        string className = selectedClass.name;

        GameObject player = PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPoint.position,
            Quaternion.identity,
            0,
            new object[] { className }
        );

        PhotonNetwork.LocalPlayer.TagObject = player;
    }
}
