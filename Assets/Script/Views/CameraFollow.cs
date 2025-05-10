using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    public GameObject objToFollow;

    //i'll do the teleport, max distance etc later
    private Transform cam;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        cam = transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (objToFollow == null) return;

        PhotonView pv = objToFollow.GetComponent<PhotonView>();
        if (pv != null && !pv.IsMine) return;

        Vector3 target = objToFollow.transform.position + new Vector3(0, 0, -10);
        float smoothSpeed = 0.125f;
        cam.position = Vector3.Lerp(cam.position, target, smoothSpeed);
    }

}
