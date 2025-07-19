using UnityEngine;
using Photon.Pun;

public class LatencyTest : MonoBehaviourPun
{
    private float sendTime;

    // G?i thông báo ?? ki?m tra ?? tr?
    public void SendTestPing()
    {
        if (photonView.IsMine)
        {
            sendTime = Time.time;
            photonView.RPC("ReceivePing", RpcTarget.OthersBuffered, sendTime);
        }
    }

    // Nh?n thông báo và tính ?? tr?
    [PunRPC]
    public void ReceivePing(float sentTime)
    {
        float latency = Time.time - sentTime;
        Debug.Log("Latency: " + latency * 1000 + " ms");
    }
}
