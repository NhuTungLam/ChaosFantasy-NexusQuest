using UnityEngine;
using Photon.Pun;
using TMPro;

public class LatencyChecker : MonoBehaviourPun
{
    public TextMeshProUGUI pingText;
    private float interval;
    void Update()
    {
        if(interval > 0)
        {
            interval -= Time.deltaTime;
        }
  
        else if (PhotonNetwork.IsConnected)
        {
            float ping = PhotonNetwork.GetPing();
            pingText.text = ping + "ms";
            interval = 0.5f;
        }
    }
}
