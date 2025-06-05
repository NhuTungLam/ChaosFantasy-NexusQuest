using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Photon.Pun.PhotonView))]
public class PlayerManager : MonoBehaviourPun

{
    public static PlayerManager Instance;
    public List<Transform> playerList = new List<Transform>();
    public void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        foreach (Transform t in new List<Transform>(playerList))
        {
            if (t == null)
            {
                playerList.Remove(t);
            }
        }
    }
    public Transform GetPlayer(Vector2 origin, float range = 5f)
    {
        Transform closestPlayer = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform player in playerList)
        {
            if (player == null) continue;

            float distance = Vector2.Distance(origin, player.position);
            if (distance <= range && distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }
    [PunRPC]
    public void RPC_AddPlayerToList(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null && view.transform != null)
        {
            if (!playerList.Contains(view.transform))
            {
                playerList.Add(view.transform);
                Debug.Log("[PlayerManager] RPC synced player: " + view.transform.name);
            }
        }
    }

}
