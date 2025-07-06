using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCardBase : MonoBehaviourPun,IInteractable
{
    [Header("Data card")]
    public bool isActiveCard = false;
    public string CardName;
    public Sprite Icon;
    public string CardDescription;
    public float cooldown = 2f;
    [Header("Runtime Data")]
    public bool hasPick = false;


    public virtual void Initialize(CharacterHandler player)
    {
        hasPick = true;
        if (isActiveCard == true)
        {
            player.SetActiveSkill(this);
        }
        else
        {
            player.SetPassiveSkill(this);
        }
    }
    public virtual void OnRemoveSkill(CharacterHandler player)
    {

    }
    public virtual void Activate(CharacterHandler player) 
    { 
    }
    public void Interact(CharacterHandler player)
    {
        if (hasPick) return;

        Initialize(player);

        PhotonView pv = GetComponent<PhotonView>();
        if (pv == null) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // ✅ Nếu là master thì có quyền gọi trực tiếp
            pv.RPC("RPC_DestroySkillCard", RpcTarget.AllBuffered);
        }
        else
        {
            // ❗ Nếu là client → gửi yêu cầu cho master
            photonView.RPC("RPC_RequestDestroySkillCard", RpcTarget.MasterClient, pv.ViewID);
        }
    }

    [PunRPC]
    public void RPC_RequestDestroySkillCard(int viewID)
    {
        PhotonView target = PhotonView.Find(viewID);
        if (target != null)
        {
            target.RPC("RPC_DestroySkillCard", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void RPC_DestroySkillCard()
    {
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public bool CanInteract()
    {
        return !hasPick;
    }
    public void InRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.ShowPickup(CardName, transform.position);
    }
    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }
}
