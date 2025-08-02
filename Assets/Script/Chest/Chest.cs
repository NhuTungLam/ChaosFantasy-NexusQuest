using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class Chest : MonoBehaviourPun, IInteractable
{
    private bool isOpen = false;
    private ChestData data;

    public void ApplyData(ChestData chestData)
    {
        data = chestData;
    }

    void Start()
    {
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            string chestDataName = (string)photonView.InstantiationData[0];
            ChestData loadedData = Resources.Load<ChestData>("Scriptable Object/Chest/" + chestDataName);
            if (loadedData != null)
            {
                ApplyData(loadedData);
            }
            else
            {
                Debug.LogError("Failed to load ChestData: " + chestDataName);
            }
        }
    }

    public void Interact(CharacterHandler player = null)
    {
        if (isOpen) return;

        // ? Always let MasterClient handle opening
        photonView.RPC("RPC_RequestOpenChest", RpcTarget.MasterClient, photonView.ViewID);
    }

    public bool CanInteract() => !isOpen;

    [PunRPC]
    public void RPC_RequestOpenChest(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;  

        photonView.RPC("RPC_OpenChestAndSpawnLoot", RpcTarget.All);

    }


    [PunRPC]
    public void RPC_OpenChestAndSpawnLoot()
    {
        if (isOpen) return;
        isOpen = true;

        Debug.Log("Chest opened: " + data?.name);

        if (!PhotonNetwork.IsMasterClient) return;

        if (data.weaponItemNames != null && Random.value < data.weaponDropRate)
        {
            string name = data.weaponItemNames[Random.Range(0, data.weaponItemNames.Length)];
            ItemSpawnManager.Instance.SpawnItem(ItemType.Weapon, name, transform.position);
        }

        if (data.passiveSkillCards != null && Random.value < data.passiveSkillDropRate)
        {
            string name = data.passiveSkillCards[Random.Range(0, data.passiveSkillCards.Length)].name;
            ItemSpawnManager.Instance.SpawnItem(ItemType.PassiveSkill, name, transform.position + Vector3.right);
        }

        if (data.activeSkillCards != null && Random.value < data.activeSkillDropRate)
        {
            string name = data.activeSkillCards[Random.Range(0, data.activeSkillCards.Length)].name;
            ItemSpawnManager.Instance.SpawnItem(ItemType.ActiveSkill, name, transform.position + Vector3.left);
        }

        if (data.otherItems != null && Random.value < data.otherItemDropRate)
        {
            string name = data.otherItems[Random.Range(0, data.otherItems.Length)].name;
            ItemSpawnManager.Instance.SpawnItem(ItemType.Other, name, transform.position + Vector3.up);
        }

        PhotonNetwork.Destroy(gameObject);
    }

    public void InRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.ShowPickup("Chest", transform.position);
    }

    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }
}
