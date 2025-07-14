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

        if (RoomSessionManager.Instance.IsRoomOwner())
        {
            OpenChestAndSpawnLoot();
        }
        else
        {
            photonView.RPC("RPC_RequestOpenChest", RpcTarget.MasterClient, photonView.ViewID);
        }
    }

    public bool CanInteract() => !isOpen;

    [PunRPC]
    public void RPC_RequestOpenChest(int viewID)
    {
        if (!RoomSessionManager.Instance.IsRoomOwner()) return;
        photonView.RPC("RPC_OpenChestAndSpawnLoot", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_OpenChestAndSpawnLoot()
    {
        if (isOpen) return;
        OpenChestAndSpawnLoot();
    }

    private void OpenChestAndSpawnLoot()
    {
        isOpen = true;
        Debug.Log("Chest opened: " + data?.name);

        //e.g. rate: 1.2f
        //var rate = 1.2f;
        //while (rate > 0)
        //{
        //    if (rate >= 1)
        //    {
        //        Spawn();
        //    }
        //    else if (Random.value < rate)
        //    {
        //        Spawn();
        //    }
        //    rate -= 1;
        //}

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


        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }


    private void SpawnLootNetwork()
    {
        if (data == null)
        {
            Debug.LogError("Chest data is null");
            return;
        }

        if (data.weaponItemNames != null && data.weaponItemNames.Length > 0 && Random.value < data.weaponDropRate)
        {
            string weaponName = data.weaponItemNames[Random.Range(0, data.weaponItemNames.Length)];
            WeaponSyncManager.Instance.SpawnWeapon(weaponName, transform.position);
        }

        photonView.RPC("RPC_OpenChestAndSpawnLoot", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_SpawnLootBatch(string[] itemNames, Vector3[] positions)
    {
        for (int i = 0; i < itemNames.Length; i++)
        {
            GameObject prefab = Resources.Load<GameObject>("Weapon/" + itemNames[i]);
            if (prefab != null)
                Instantiate(prefab, positions[i], Quaternion.identity);
        }
    }

    private Vector3 GetOffsetByIndex(int index, int total, float radius = 1f)
    {
        float angle = 360f / total * index;
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
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
