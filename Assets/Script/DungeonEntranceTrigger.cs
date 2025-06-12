using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class DungeonEntranceTrigger : MonoBehaviour, IInteractable
{
    //private bool playerInZone = false;
    public string dungeonRoomScene = "Enter_Dungeon"; // Scene trung gian để tạo room dungeon

    void Update()
    {
        
    }

    IEnumerator LeaveRoomThenLoadLobbyScene()
    {
        PhotonRoomManager.skipAutoCreateRoom = true;

        if (PhotonNetwork.InRoom)
        {
            // ✅ Dọn player trước khi rời room
            if (PhotonNetwork.LocalPlayer.TagObject is GameObject go)
            {
                PhotonNetwork.Destroy(go);
                PhotonNetwork.LocalPlayer.TagObject = null;
            }

            PhotonNetwork.LeaveRoom();

            float timeout = 5f;
            while (PhotonNetwork.InRoom && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        float waitMasterTimeout = 5f;
        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer && waitMasterTimeout > 0f)
        {
            waitMasterTimeout -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("🏁 Loading Enter_Dungeon...");
        SceneManager.LoadScene("Enter_Dungeon");
    }

    public bool CanInteract()
    {
        return true;
    }
    public void Interact(CharacterHandler user = null)
    {
        StartCoroutine(LeaveRoomThenLoadLobbyScene());
    }
    public void InRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.ShowPickup("Enter Dungeon", transform.position);
    }
    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }

}
