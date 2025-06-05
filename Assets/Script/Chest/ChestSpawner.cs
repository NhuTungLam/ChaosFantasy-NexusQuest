using UnityEngine;

public class ChestSpawner : MonoBehaviour
{

    public GameObject chestPrefab;
    public Vector3 spawnPosition;
    private bool hasSpawned = false;


    public void SpawnChest()
    {
        if (hasSpawned || chestPrefab == null) return;

        Instantiate(chestPrefab, spawnPosition, Quaternion.identity);
        hasSpawned = true;
    }

    public void ResetSpawner()
    {
        hasSpawned = false;
    }

    public void MarkChestAsGone()
    {
        hasSpawned = false;
    }

}

