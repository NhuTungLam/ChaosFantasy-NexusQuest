using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Gán Player vào đây
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // Dùng nếu muốn đặt camera lệch

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
