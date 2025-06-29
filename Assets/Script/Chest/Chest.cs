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

        if (data.weaponItemNames != null && data.weaponItemNames.Length > 0 && Random.value < data.weaponDropRate)
        {
            string weaponName = data.weaponItemNames[Random.Range(0, data.weaponItemNames.Length)];
            WeaponSyncManager.Instance.SpawnWeapon(weaponName, transform.position);
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
