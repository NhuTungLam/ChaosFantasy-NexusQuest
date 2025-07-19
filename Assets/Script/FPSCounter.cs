using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        Debug.Log("FPS: " + fps);
    }
}
