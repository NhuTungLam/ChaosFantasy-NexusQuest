using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public RoomStateManager roomStateManager; 

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void OnResumeButton()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnExitToNexusButton()
    {
        // ✅ Lưu room trước khi rời
        if (roomStateManager != null)
        {
            roomStateManager.SaveRoomToServer();
        }
        else
        {
            Debug.LogWarning("[GameManager] RoomStateManager not assigned!");
        }

        StartCoroutine(LeaveRoomAndLoadNexus());
    }

    private IEnumerator LeaveRoomAndLoadNexus()
    {
        Time.timeScale = 1; // reset nếu đang pause

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.LocalPlayer.TagObject is GameObject go)
            {
                PhotonNetwork.Destroy(go);
                PhotonNetwork.LocalPlayer.TagObject = null;
            }

            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom)
                yield return null;

            yield return new WaitForSeconds(0.2f);
        }

        PhotonRoomManager.skipAutoCreateRoom = false;
        Debug.Log("🔁 Returning to Nexus...");
        SceneManager.LoadScene("Nexus");
    }
}
