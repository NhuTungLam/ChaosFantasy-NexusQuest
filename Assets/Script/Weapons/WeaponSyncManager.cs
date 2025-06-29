using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSyncManager : MonoBehaviourPunCallbacks
{
    public static WeaponSyncManager Instance;
    private List<WeaponState> activeWeapons = new List<WeaponState>();

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnWeapon(string weaponType, Vector3 position)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        string id = System.Guid.NewGuid().ToString();
        WeaponState ws = new WeaponState { id = id, type = weaponType, position = position };
        activeWeapons.Add(ws);
        InstantiateWeaponLocal(ws);
        photonView.RPC("RPC_SpawnWeapon", RpcTarget.Others, JsonUtility.ToJson(ws));
    }

    public void SpawnWeaponWithState(WeaponState ws)
    {
        activeWeapons.Add(ws);
        InstantiateWeaponLocal(ws);
        photonView.RPC("RPC_SpawnWeapon", RpcTarget.Others, JsonUtility.ToJson(ws));
    }

    [PunRPC]
    void RPC_SpawnWeapon(string json)
    {
        WeaponState ws = JsonUtility.FromJson<WeaponState>(json);
        InstantiateWeaponLocal(ws);
    }

    public void PickupWeaponWithDrop(string pickupWeaponId, int viewID, string pickupWeaponType, string dropWeaponType, Vector3 dropPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        activeWeapons.RemoveAll(w => w.id == pickupWeaponId);
        photonView.RPC("RPC_RemoveWeapon", RpcTarget.All, pickupWeaponId);

        if (!string.IsNullOrEmpty(dropWeaponType))
        {
            string newId = System.Guid.NewGuid().ToString();
            WeaponState dropState = new WeaponState { id = newId, type = dropWeaponType, position = dropPosition };
            activeWeapons.Add(dropState);
            photonView.RPC("RPC_SpawnWeapon", RpcTarget.All, JsonUtility.ToJson(dropState));
        }

        photonView.RPC("RPC_EquipWeapon", RpcTarget.All, viewID, pickupWeaponType);
    }
    [PunRPC]
    void RPC_RequestPickupWeapon(string pickupWeaponId, int viewID, string pickupWeaponType, string dropWeaponType, Vector3 dropPosition)
    {
        PickupWeaponWithDrop(pickupWeaponId, viewID, pickupWeaponType, dropWeaponType, dropPosition);
    }

    [PunRPC]
    void RPC_EquipWeapon(int viewID, string weaponType)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null) return;
        var handler = view.GetComponent<CharacterHandler>();
        if (handler == null) return;

        GameObject weaponPrefab = Resources.Load<GameObject>("Weapon/" + weaponType);
        if (weaponPrefab != null)
        {
            var weaponObj = Instantiate(weaponPrefab);
            string newId = System.Guid.NewGuid().ToString();
            weaponObj.GetComponent<WeaponBase>().Initialize(newId);
            handler.EquipWeapon(weaponObj.GetComponent<WeaponBase>());
        }
    }

    [PunRPC]
    void RPC_RemoveWeapon(string weaponId)
    {
        var allWeapons = GameObject.FindObjectsOfType<WeaponBase>();
        foreach (var w in allWeapons)
        {
            if (w.weaponId == weaponId)
            {
                Destroy(w.gameObject);
                break;
            }
        }
    }

    private void InstantiateWeaponLocal(WeaponState ws)
    {
        GameObject prefab = Resources.Load<GameObject>("Weapon/" + ws.type);
        var obj = Instantiate(prefab, ws.position, Quaternion.identity);
        var weaponBase = obj.GetComponent<WeaponBase>();
        weaponBase.Initialize(ws.id);
    }
}

[System.Serializable]
public class WeaponState
{
    public string id;
    public string type;
    public Vector3 position;
}
