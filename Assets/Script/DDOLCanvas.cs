using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DDOLCanvas : MonoBehaviour
{
    private static DDOLCanvas instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate if already exists
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += (s, a) =>
        {
            if (TryGetComponent<Canvas>(out var canvas))
            {
                canvas.worldCamera = Camera.main;
            }
        };
    }
}
