using UnityEngine;
using System.Collections.Generic;

public enum RoomType { Normal, Boss, Special }

public class Room : MonoBehaviour
{
    [Header("Room Setup")]
    public RoomType roomType;
    public List<EnewaveData> enewaveDatas = new();
    public bool needCloseDoor = true;

    [Header("Chest")]
    public ChestData chestData;
    public Transform chestSpawnPoint;

    [Header("Doors")]
    public GameObject topDoor, bottomDoor, leftDoor, rightDoor;

    private List<Direction> directions = new();
    private bool isTriggered = false;
    private bool doorsClosed = false;
    private bool chestSpawned = false;

    public EnemySpawner enemySpawner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered || !other.CompareTag("Player")) return;

        isTriggered = true;

        if (needCloseDoor)
        {
            getActiveDoor();
            CloseDoors();
            EnemySpawner.Instance.RoomSpawner(enewaveDatas, transform.position);
        }
    }

    private void Update()
    {
        if (doorsClosed && EnemySpawner.Instance.AllEnemiesDefeated)
        {
            OpenDoors();
            doorsClosed = false;

            if (!chestSpawned && chestData != null && chestSpawnPoint != null)
            {
                SpawnChest();
                chestSpawned = true;
            }

            Debug.Log("[Room] Cleared - Opened doors & spawned chest.");
        }
    }

    private void SpawnChest()
    {
        GameObject chestObj = Instantiate(chestData.chestPrefab, chestSpawnPoint.position, Quaternion.identity);
        Chest chest = chestObj.GetComponent<Chest>();
        if (chest != null)
        {
            chest.ApplyData(chestData);
        }
    }

    public void getActiveDoor()
    {
        directions.Clear();
        if (!IsDoorOpen(Direction.Top)) directions.Add(Direction.Top);
        if (!IsDoorOpen(Direction.Bottom)) directions.Add(Direction.Bottom);
        if (!IsDoorOpen(Direction.Left)) directions.Add(Direction.Left);
        if (!IsDoorOpen(Direction.Right)) directions.Add(Direction.Right);
    }

    private void CloseDoors()
    {
        foreach (var dir in directions)
        {
            EnableDoor(dir);
        }
        doorsClosed = true;
        Debug.Log("[Room] Closed doors.");
    }

    private void OpenDoors()
    {
        foreach (var dir in directions)
        {
            DisableDoor(dir);
        }
    }

    public void EnableDoor(Direction dir)
    {
        GetDoor(dir)?.SetActive(true);
    }

    public void DisableDoor(Direction dir)
    {
        GetDoor(dir)?.SetActive(false);
    }

    public bool IsDoorOpen(Direction dir)
    {
        return GetDoor(dir)?.activeSelf ?? false;
    }

    private GameObject GetDoor(Direction dir)
    {
        return dir switch
        {
            Direction.Top => topDoor,
            Direction.Bottom => bottomDoor,
            Direction.Left => leftDoor,
            Direction.Right => rightDoor,
            _ => null
        };
    }

}

public enum Direction { Top, Bottom, Left, Right }

public static class DirectionHelper
{
    public static Vector2Int ToVector2Int(Direction dir)
    {
        return dir switch
        {
            Direction.Top => new Vector2Int(0, 1),
            Direction.Bottom => new Vector2Int(0, -1),
            Direction.Left => new Vector2Int(-1, 0),
            Direction.Right => new Vector2Int(1, 0),
            _ => Vector2Int.zero
        };
    }

    public static Direction GetOpposite(Direction dir)
    {
        return dir switch
        {
            Direction.Top => Direction.Bottom,
            Direction.Bottom => Direction.Top,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.Top
        };
    }
}
