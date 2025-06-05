using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    public GameObject objToFollow;
    private Transform cam;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = transform;
        if (objToFollow == null)
            Invoke(nameof(FindTarget), 0.2f);
    }

    void FindTarget()
    {
        objToFollow = GameObject.FindWithTag("Player");
    }

    void LateUpdate()
    {
        if (objToFollow == null) return;

        PhotonView pv = objToFollow.GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.InRoom && !pv.IsMine) return;

        Vector3 target = objToFollow.transform.position + new Vector3(0, 0, -10);
        float smoothSpeed = 1f;
        cam.position = Vector3.Lerp(cam.position, target, smoothSpeed);
    }
}
