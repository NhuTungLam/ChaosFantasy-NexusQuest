using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class DungeonEntranceTrigger : MonoBehaviour
{
    private bool playerInZone = false;
    public string dungeonRoomScene = "Enter_Dungeon"; // Scene trung gian để tạo room dungeon

    void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("🚪 Entering dungeon door → leave Nexus & go to dungeon UI");
            StartCoroutine(LeaveRoomThenLoadLobbyScene());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInZone = false;
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

}
