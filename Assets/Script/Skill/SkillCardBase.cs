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

        // Gán skill
        if (isActiveCard)
            player.SetActiveSkill(this);
        else
            player.SetPassiveSkill(this);

        // Đảm bảo là con của player
        transform.SetParent(player.transform, false);
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

        Initialize(player); // Gán skill vào player và làm con trong hàm này

        PhotonView pv = GetComponent<PhotonView>();
        if (pv == null) return;

        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("RPC_HideSkillCard", RpcTarget.AllBuffered); // dùng RPC để ẩn
        }
        else
        {
            photonView.RPC("RPC_RequestHideSkillCard", RpcTarget.MasterClient, pv.ViewID);
        }
    }

    [PunRPC]
    public void RPC_RequestHideSkillCard(int viewID)
    {
        PhotonView target = PhotonView.Find(viewID);
        if (target != null)
        {
            target.RPC("RPC_HideSkillCard", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void RPC_HideSkillCard()
    {
        if (TryGetComponent<SpriteRenderer>(out var sr))
            sr.enabled = false;

        if (TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        // Tắt script nếu cần
        //enabled = false;
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
