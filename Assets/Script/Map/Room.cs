using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{

    public bool needCloseDoor;
    public List<Direction> directions = new List<Direction>();

    public GameObject topDoor, bottomDoor, leftDoor, rightDoor;
    private bool isTriggered = false;
    private bool doorsClosed = false;
    public List<EnewaveData> enewaveDatas = new List<EnewaveData>();
    private void Awake()
    {
        
    }


    public bool IsDoorOpen(Direction direction)
    {
        return GetDoor(direction).activeSelf;
    }

    public void EnableDoor(Direction dir)
    {
        GetDoor(dir)?.SetActive(true);
    }

    public void DisableDoor(Direction dir)
    {
        GetDoor(dir)?.SetActive(false);
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

    public void getActiveDoor()
    {
        directions.Clear();
        if (!IsDoorOpen(Direction.Top)) directions.Add(Direction.Top);
        if (!IsDoorOpen(Direction.Bottom)) directions.Add(Direction.Bottom);
        if (!IsDoorOpen(Direction.Left)) directions.Add(Direction.Left);
        if (!IsDoorOpen(Direction.Right)) directions.Add(Direction.Right);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;

            if (needCloseDoor)
            {
                getActiveDoor();
                CloseDoors();
                EnemySpawner.Instance.RoomSpawner(enewaveDatas, transform.position);
            }
        }
    }

    private void Update()
    {
        if (doorsClosed && EnemySpawner.Instance.AllEnemiesDefeated)
        {
            OpenDoors();
            doorsClosed = false;
            Debug.Log("[Room] Opened doors after clearing enemies.");
        }
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
