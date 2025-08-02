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

        // ✅ Không áp dụng load save ở đây nếu không phải local player
        if (photonView.IsMine)
        {
            if (DungeonRestorerManager.Instance != null && DungeonRestorerManager.Instance.playerinfo != null)
            {
                handler.ApplyLoadSave(DungeonRestorerManager.Instance.playerinfo);
            }
            else
            {
                handler.Init(characterData);
            }

            PhotonNetwork.LocalPlayer.TagObject = this.gameObject;

            if (CameraFollow.Instance != null)
                CameraFollow.Instance.objToFollow = this.gameObject;
        }
        else
        {
            // 🔁 Remote player chỉ init visual, sau đó chủ phòng gọi RPC_LoadTeammateVisual
            handler.Init(characterData); // tạm thời init để render
        }

        Debug.Log($"✅ [{(photonView.IsMine ? "Local" : "Remote")}] Player Init with class: {className}");
    }

}
