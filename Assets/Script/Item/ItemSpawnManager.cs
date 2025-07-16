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
        string path = type switch
        {
            ItemType.Weapon => "Weapon/" + name,
            ItemType.PassiveSkill => "SkillCard/Passive/" + name,
            ItemType.ActiveSkill => "SkillCard/Active/" + name,
            ItemType.Other => "Item/" + name,
            _ => null
        };

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[ItemSpawnManager] Invalid item path: " + name);
            return;
        }

        if (type == ItemType.Weapon)
        {
            // ✅ Spawn weapon locally (no PhotonView needed)
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("[ItemSpawnManager] Weapon prefab not found: " + path);
                return;
            }

            GameObject weaponObj = Instantiate(prefab, position, Quaternion.identity);
            string weaponId = Guid.NewGuid().ToString();

            if (weaponObj.TryGetComponent<WeaponBase>(out var weapon))
            {
                weapon.Initialize(weaponId);
                Debug.Log($"🗡️ [ItemSpawnManager] Locally spawned weapon: {name} ({weaponId}) at {position}");
            }
            else
            {
                Debug.LogWarning($"[ItemSpawnManager] Weapon prefab missing WeaponBase: {name}");
            }
        }
        else
        {
            // ✅ Only MasterClient can spawn non-weapon items
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning($"⛔ [ItemSpawnManager] Only MasterClient can spawn item: {name}");
                return;
            }

            Debug.Log($"✨ [ItemSpawnManager] Master spawning item: {name} ({type}) at {position}");
            PhotonNetwork.Instantiate(path, position, Quaternion.identity);
        }
    }
}
