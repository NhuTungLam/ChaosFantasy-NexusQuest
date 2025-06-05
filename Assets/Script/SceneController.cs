using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class SceneController : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        // B?t ??u connect Photon n?u ch?a k?t n?i
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("?? SceneController: Connecting to Photon...");
        }

        SceneManager.LoadScene(sceneName);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
