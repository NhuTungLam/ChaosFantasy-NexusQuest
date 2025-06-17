using UnityEngine;
using Photon.Pun;

public class PlayerNetworkInit : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private CharacterHandler handler;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        handler = GetComponent<CharacterHandler>();

        object[] data = photonView.InstantiationData;
        string className = (string)data[0];

        CharacterData characterData = Resources.Load<CharacterData>("Characters/" + className);
        if (characterData == null)
        {
            Debug.LogError("❌ Không tìm thấy CharacterData: " + className);
            return;
        }
        if (DungeonRestorerManager.Instance != null && DungeonRestorerManager.Instance.playerinfo != null)
        {
            
            handler.ApplyLoadSave(DungeonRestorerManager.Instance.playerinfo);
        }
        else
        {
            handler.Init(characterData);
        }
        Debug.Log($"✅ [{(photonView.IsMine ? "Local" : "Remote")}] Player Init with class: {className}");

        if (photonView.IsMine)
        {
            // ⚠️ Gán chính xác TagObject TẠI ĐÂY, chỉ cho player local
            PhotonNetwork.LocalPlayer.TagObject = this.gameObject;

            if (CameraFollow.Instance != null)
                CameraFollow.Instance.objToFollow = this.gameObject;
        }
    }

}
