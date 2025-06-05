using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class RoomBSPDungeonGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject bossRoomPrefab;
    [SerializeField] private GameObject specialRoomPrefab;
    [SerializeField] private Tilemap corridorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase corridorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField] private int dungeonWidth = 50, dungeonHeight = 50;
    [SerializeField] private int maxSpecialRooms = 2;
    [SerializeField] private Vector2Int roomSize = new Vector2Int(10, 10);

    private List<Vector2Int> roomCenters = new();
    private Dictionary<Vector2Int, GameObject> roomInstances = new();
    private Dictionary<Vector2Int, Direction> roomEntryDirs = new();
    private HashSet<Vector3Int> corridorPositions = new();
    private List<Vector2Int> deadEnds = new();

    private static readonly List<Vector3Int> directions8 = new()
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(1, 1, 0), new Vector3Int(-1, 1, 0),
        new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0)
    };

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        ClearOldRooms();
        List<BoundsInt> rooms = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt(Vector3Int.zero, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth, minRoomHeight);

        foreach (var room in rooms)
        {
            Vector2Int center = (Vector2Int)Vector3Int.RoundToInt(room.center);
            Vector3 worldPos = new Vector3(center.x * roomSize.x, center.y * roomSize.y, 0);
            GameObject roomGO = Instantiate(roomPrefab, worldPos, Quaternion.identity);
            roomGO.name = "Room_" + center;
            roomCenters.Add(center);
            roomInstances.Add(center, roomGO);
        }

        ConnectRooms();
        GenerateCorridorWalls();
        PlaceSpecialRooms();
    }

    private void ConnectRooms()
    {
        var remaining = new List<Vector2Int>(roomCenters);
        Vector2Int current = remaining[UnityEngine.Random.Range(0, remaining.Count)];
        remaining.Remove(current);

        HashSet<Vector2Int> visited = new() { current };
        Dictionary<Vector2Int, List<Vector2Int>> graph = new();

        while (remaining.Count > 0)
        {
            Vector2Int closest = FindClosest(current, remaining);
            remaining.Remove(closest);
            visited.Add(closest);

            Direction entryDir = GetDirection(current, closest);
            roomEntryDirs[closest] = entryDir;

            if (roomInstances.TryGetValue(current, out GameObject currentRoom))
            {
                currentRoom.GetComponent<Room>().EnableDoor(entryDir);
            }
            if (roomInstances.TryGetValue(closest, out GameObject nextRoom))
            {
                nextRoom.GetComponent<Room>().EnableDoor(DirectionHelper.GetOpposite(entryDir));
               // nextRoom.GetComponent<Room>().entryDirection = DirectionHelper.GetOpposite(entryDir); // ?
            }


            if (!graph.ContainsKey(current)) graph[current] = new();
            if (!graph.ContainsKey(closest)) graph[closest] = new();
            graph[current].Add(closest);
            graph[closest].Add(current);

            CreateCorridorBetween(current, closest);

            current = closest;
        }

        foreach (var node in graph)
        {
            if (node.Value.Count == 1)
                deadEnds.Add(node.Key);
        }
    }

    private Direction GetDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? Direction.Right : Direction.Left;
        else
            return delta.y > 0 ? Direction.Top : Direction.Bottom;
    }

    private void CreateCorridorBetween(Vector2Int a, Vector2Int b)
    {
        Vector3Int start = new Vector3Int(a.x * roomSize.x, a.y * roomSize.y, 0);
        Vector3Int end = new Vector3Int(b.x * roomSize.x, b.y * roomSize.y, 0);

        Vector3Int current = start;

        while (current.x != end.x)
        {
            SetCorridorTile(current);
            current.x += end.x > current.x ? 1 : -1;
        }

        while (current.y != end.y)
        {
            SetCorridorTile(current);
            current.y += end.y > current.y ? 1 : -1;
        }

        SetCorridorTile(current);
    }

    private void SetCorridorTile(Vector3Int position)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int offset = new Vector3Int(position.x + x, position.y + y, 0);
                corridorTilemap.SetTile(offset, corridorTile);
                corridorPositions.Add(offset);
            }
        }
    }

    private void GenerateCorridorWalls()
    {
        foreach (var pos in corridorPositions)
        {
            foreach (var dir in directions8)
            {
                Vector3Int neighbor = pos + dir;
                if (!corridorPositions.Contains(neighbor) && corridorTilemap.GetTile(neighbor) == null)
                {
                    wallTilemap.SetTile(neighbor, wallTile);
                }
            }
        }
    }

    private Vector2Int FindClosest(Vector2Int current, List<Vector2Int> candidates)
    {
        Vector2Int closest = Vector2Int.zero;
        float minDist = float.MaxValue;
        foreach (var pos in candidates)
        {
            float dist = Vector2.Distance(current, pos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = pos;
            }
        }
        return closest;
    }

    private void PlaceSpecialRooms()
    {
        if (deadEnds.Count == 0) return;

        // Boss room t?i dead-end xa nh?t
        Vector2Int bossPos = deadEnds.OrderByDescending(p => Vector2Int.Distance(Vector2Int.zero, p)).First();
        if (roomEntryDirs.TryGetValue(bossPos, out Direction bossDir))
        {
            ReplaceRoom(bossRoomPrefab, bossPos, bossDir);
        }

        // Special room t?i các dead-end còn l?i
        int placed = 0;
        foreach (var pos in deadEnds)
        {
            if (pos == bossPos) continue;

            if (roomEntryDirs.TryGetValue(pos, out Direction dir))
            {
                ReplaceRoom(specialRoomPrefab, pos, dir);
                placed++;
                if (placed >= maxSpecialRooms) break;
            }
        }
    }



    private void ReplaceRoom(GameObject prefab, Vector2Int pos, Direction openDirection)
    {
        if (!roomInstances.TryGetValue(pos, out GameObject oldRoom)) return;

        Vector3 position = oldRoom.transform.position;
        Quaternion rotation = oldRoom.transform.rotation;
        DestroyImmediate(oldRoom);

        GameObject newRoom = Instantiate(prefab, position, rotation);
        newRoom.name = prefab.name + "_" + pos;

        Room room = newRoom.GetComponent<Room>();
        //room.entryDirection = openDirection;
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (dir == openDirection)
                room.EnableDoor(dir);
            else
                room.DisableDoor(dir);
        }

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Vector2Int neighborPos = pos + DirectionHelper.ToVector2Int(dir);
            bool isCorrectDir = dir == openDirection;

            if (roomInstances.TryGetValue(neighborPos, out GameObject neighborRoom))
            {
                Room neighbor = neighborRoom.GetComponent<Room>();
                if (!isCorrectDir)
                {
                    neighbor.DisableDoor(DirectionHelper.GetOpposite(dir));
                }
            }

            if (!isCorrectDir)
            {
                Vector2Int from = pos;
                Vector2Int to = neighborPos;
                Vector3Int start = new Vector3Int(from.x * roomSize.x, from.y * roomSize.y, 0);
                Vector3Int end = new Vector3Int(to.x * roomSize.x, to.y * roomSize.y, 0);

                Vector3Int current = start;
                while (current != end)
                {
                    corridorTilemap.SetTile(current, null);
                    corridorPositions.Remove(current);

                    if (current.x != end.x)
                        current.x += end.x > current.x ? 1 : -1;
                    else if (current.y != end.y)
                        current.y += end.y > current.y ? 1 : -1;
                }

                corridorTilemap.SetTile(end, null);
                corridorPositions.Remove(end);
            }
        }

        roomInstances[pos] = newRoom;
    }



    private void ClearOldRooms()
{
    foreach (var kvp in roomInstances)
    {
        if (kvp.Value != null)
            DestroyImmediate(kvp.Value);
    }
    roomInstances.Clear();
    roomCenters.Clear();
    deadEnds.Clear();
    roomEntryDirs.Clear();

    corridorTilemap.ClearAllTiles();
    wallTilemap.ClearAllTiles();
    corridorPositions.Clear();
}
}
