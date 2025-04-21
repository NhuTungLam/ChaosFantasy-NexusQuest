using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject objToFollow;

    //i'll do the teleport, max distance etc later
    private Transform cam;
    void Start()
    {
        cam = transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 target = objToFollow.transform.position + new Vector3(0, 0, -10);
        float smoothSpeed = 1f; // càng nhỏ càng mượt
        cam.position = Vector3.Lerp(cam.position, target, smoothSpeed);
    }

}
