using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class DontDestroyOnLoad : MonoBehaviour
{
    [Tooltip("Unique identifier for this persistent object.")]
    public string persistentID;

    private static Dictionary<string, DontDestroyOnLoad> instances = new();

    void Awake()
    {
        if (string.IsNullOrEmpty(persistentID))
        {
            Debug.LogWarning($"[DontDestroyByID] GameObject '{name}' has no persistentID set. Skipping persistence.");
            return;
        }

        if (instances.ContainsKey(persistentID))
        {
            Debug.Log($"[DontDestroyByID] Duplicate detected for ID '{persistentID}'. Destroying new one: {gameObject.name}");
            Destroy(gameObject);
        }
        else
        {
            instances[persistentID] = this;
            DontDestroyOnLoad(gameObject);

        }
        SceneManager.activeSceneChanged += (s, a) =>
        {
            if (TryGetComponent<Canvas>(out var canvas))
            {
                canvas.worldCamera = Camera.main;
            }
        };
    }

    void OnDestroy()
    {
        if (instances.ContainsKey(persistentID) && instances[persistentID] == this)
        {
            instances.Remove(persistentID);
        }
    }
}
