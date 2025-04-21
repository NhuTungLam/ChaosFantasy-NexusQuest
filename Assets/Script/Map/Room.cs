using UnityEngine;
public class Room : MonoBehaviour
{
    public bool IsDoorOpen(Direction direction)
    {
        return GetDoor(direction).activeSelf;
    }
    public GameObject topDoor, bottomDoor, leftDoor, rightDoor;

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
}
public enum Direction { Top, Bottom, Left, Right }

public static class DirectionHelper
{
    public static Vector2Int ToVector2Int(Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                return new Vector2Int(0, 1);
            case Direction.Bottom:
                return new Vector2Int(0, -1);
            case Direction.Left:
                return new Vector2Int(-1, 0);
            case Direction.Right:
                return new Vector2Int(1, 0);
        }
        return new Vector2Int(0, 1);
    }

    public static Direction GetOpposite(Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                return Direction.Bottom;
            case Direction.Bottom:
                return Direction.Top;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
        }
        return Direction.Top;
    }
}