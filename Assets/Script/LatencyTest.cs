using UnityEngine;
using Photon.Pun;

public class LatencyTest : MonoBehaviourPun
{
    private float sendTime;

    // G?i th�ng b�o ?? ki?m tra ?? tr?
    public void SendTestPing()
    {
        if (photonView.IsMine)
        {
            sendTime = Time.time;
            photonView.RPC("ReceivePing", RpcTarget.OthersBuffered, sendTime);
        }
    }

    // Nh?n th�ng b�o v� t�nh ?? tr?
    [PunRPC]
    public void ReceivePing(float sentTime)
    {
        float latency = Time.time - sentTime;
        Debug.Log("Latency: " + latency * 1000 + " ms");
    }
}
