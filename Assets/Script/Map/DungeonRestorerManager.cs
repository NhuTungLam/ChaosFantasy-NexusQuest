using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRestorerManager : MonoBehaviour
{
    public static DungeonRestorerManager Instance { get; private set; }
    public DungeonApiClient.DungeonProgressDTO dungeoninfo;
    public DungeonApiClient.PlayerProgressDTO playerinfo;
    public bool loadSave = false;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ResetState();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetState()
    {
        loadSave = false;
        dungeoninfo = null; 
        playerinfo = null; 
    }
}
