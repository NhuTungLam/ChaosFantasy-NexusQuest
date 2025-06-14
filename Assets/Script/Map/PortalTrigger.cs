using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        if (PhotonNetwork.IsMasterClient)
        {
            int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
            int nextStage = currentStage + 1;
            PlayerPrefs.SetInt("CurrentStage", nextStage);
            PlayerPrefs.DeleteKey("SavedDungeonLayout"); 

            // Reload dungeon scene ? GenerateDungeon s? sinh layout m?i
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }
}
