using UnityEngine;
using Photon.Pun;
using System;

public enum ItemType { Weapon, PassiveSkill, ActiveSkill, Other }

[Serializable]
public class ItemState
{
    public string id;
    public ItemType itemType;
    public string itemName;
    public Vector3 position;
}

public class ItemSpawnManager : MonoBehaviourPunCallbacks
{
    public static ItemSpawnManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnItem(ItemType type, string name, Vector3 position)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        string id = Guid.NewGuid().ToString();
        ItemState state = new ItemState
        {
            id = id,
            itemType = type,
            itemName = name,
            position = position
        };

        InstantiateItemLocal(state);
        photonView.RPC("RPC_SpawnItem", RpcTarget.Others, JsonUtility.ToJson(state));
    }

    [PunRPC]
    private void RPC_SpawnItem(string json)
    {
        ItemState state = JsonUtility.FromJson<ItemState>(json);
        InstantiateItemLocal(state);
    }

    private void InstantiateItemLocal(ItemState state)
    {
        string path = state.itemType switch
        {
            ItemType.Weapon => "Weapon/" + state.itemName,
            ItemType.PassiveSkill => "SkillCard/Passive/" + state.itemName,
            ItemType.ActiveSkill => "SkillCard/Active/" + state.itemName,
            ItemType.Other => "Item/" + state.itemName,
            _ => null
        };

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[ItemSpawnManager] Invalid item path for: " + state.itemName);
            return;
        }

        if (state.itemType == ItemType.PassiveSkill ||
            state.itemType == ItemType.ActiveSkill ||
            state.itemType == ItemType.Other) 
        {
            PhotonNetwork.Instantiate(path, state.position, Quaternion.identity);
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                Instantiate(prefab, state.position, Quaternion.identity);
            }
        }

    }

}
