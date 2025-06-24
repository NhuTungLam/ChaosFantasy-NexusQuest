using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public enum RoomType { Normal, Boss, Special }

public class Room : MonoBehaviour
{
    public string roomname;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered || !other.CompareTag("Player")) return;
        isTriggered = true;

        if (transform.position == Vector3.zero)
            return;

        if (needCloseDoor)
        {
            getActiveDoor();
            CloseDoors();

            if (RoomSessionManager.Instance.IsRoomOwner() && PhotonEnemySpawner.Instance != null)
            {
                PhotonEnemySpawner.Instance.SpawnWave(enewaveDatas, 0, transform.position);
            }
        }
    }

    private void Update()
    {
        if (doorsClosed && PhotonEnemySpawner.Instance != null && PhotonEnemySpawner.Instance.AllEnemiesDefeated)
        {
            OpenDoors();
            doorsClosed = false;

            if (!chestSpawned && chestData != null && chestSpawnPoint != null)
            {
                SpawnChest();
                chestSpawned = true;
                if (roomType == RoomType.Boss)
                {
                    SpawnPortal();
                }
            }
        }
    }

    private void SpawnChest()
    {
        GameObject chestObj = Instantiate(chestData.chestPrefab, chestSpawnPoint.position, Quaternion.identity);
        Chest chest = chestObj.GetComponent<Chest>();
        if (chest != null) chest.ApplyData(chestData);
    }
    private void SpawnPortal()
    {
        var portalPrefab = Resources.Load<GameObject>("NextStagePortal");
        Instantiate(portalPrefab, chestSpawnPoint.position + new Vector3(0, 10f), Quaternion.identity);
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
        foreach (var dir in directions) EnableDoor(dir);
        doorsClosed = true;
    }

    private void OpenDoors()
    {
        foreach (var dir in directions) DisableDoor(dir);
    }

    public void EnableDoor(Direction dir) => GetDoor(dir)?.SetActive(true);
    public void DisableDoor(Direction dir) => GetDoor(dir)?.SetActive(false);
    public bool IsDoorOpen(Direction dir) => GetDoor(dir)?.activeSelf ?? false;

    public GameObject GetDoor(Direction dir)
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