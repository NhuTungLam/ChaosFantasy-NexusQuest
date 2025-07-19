using UnityEngine;
using Photon.Pun;

public class LatencyChecker : MonoBehaviourPun
{
    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            float ping = PhotonNetwork.GetPing();
            Debug.Log("Current Ping: " + ping + "ms");
        }
    }
}
