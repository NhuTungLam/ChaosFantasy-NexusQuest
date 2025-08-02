using Photon.Pun;
using UnityEngine;

public class HealthPotion : MonoBehaviourPun, IInteractable
{
    public int healAmount = 30;
    private bool isPicked = false;

    public bool CanInteract() => !isPicked;

    public void Interact(CharacterHandler user = null)
    {
        if (isPicked || user == null) return;

        isPicked = true; // ✅ Disable local interaction immediately
        user.Heal(healAmount);
        Debug.Log($"[HealthPotion] Player healed +{healAmount} HP");

        // ✅ Sync destruction
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_DestroyPotion", RpcTarget.AllBuffered);
        }
        else
        {
            photonView.RPC("RPC_RequestDestroyPotion", RpcTarget.MasterClient, photonView.ViewID);
        }
    }

    [PunRPC]
    public void RPC_RequestDestroyPotion(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            pv.RPC("RPC_DestroyPotion", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void RPC_DestroyPotion()
    {
        isPicked = true;

        // ✅ Disable interaction & visuals
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        DungeonPickup.HidePickup();

        // ✅ Delay destroy slightly for stability
        Destroy(gameObject, 0.1f);
    }

    public void InRangeAction(CharacterHandler user = null)
    {
        if (!isPicked)
            DungeonPickup.ShowPickup("HP potion", transform.position);
    }

    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }
}
