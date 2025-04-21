using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public int roomCount = 10;
    public GameObject startRoomPrefab;
    public GameObject normalRoomPrefab;
    public GameObject specialRoomPrefab;
    public GameObject bossRoomPrefab;
    public GameObject corridorPrefab;
    public int roomSpacing = 20;

    private Dictionary<Vector2Int, RoomData> dungeonMap = new();
    private Dictionary<Vector2Int, GameObject> spawnedRooms = new();

    Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down,
        Vector2Int.left, Vector2Int.right
    };

    void Start()
    {
        GenerateDungeon();
        SpawnDungeon();
    }

    void GenerateDungeon()
    {
        Vector2Int startPos = Vector2Int.zero;
        dungeonMap[startPos] = new RoomData
        {
            gridPos = startPos,
            type = RoomType.Start,
            roomPrefab = startRoomPrefab
        };

        // ➤ Tạo đúng 1 phòng nối từ Start Room
        List<Vector2Int> shuffledDirs = directions.OrderBy(x => Random.value).ToList();
        Vector2Int usedDir = shuffledDirs[0];
        Vector2Int firstRoomPos = startPos + usedDir;

        dungeonMap[firstRoomPos] = new RoomData
        {
            gridPos = firstRoomPos,
            type = RoomType.Normal,
            roomPrefab = normalRoomPrefab
        };

        List<Vector2Int> potentialDeadEnds = new List<Vector2Int> { firstRoomPos };

        for (int i = 2; i < roomCount; i++)
        {
            Vector2Int basePos = potentialDeadEnds[Random.Range(0, potentialDeadEnds.Count)];
            Vector2Int nextPos;
            int retry = 0;

            do
            {
                Vector2Int tryDir = directions[Random.Range(0, directions.Length)];
                nextPos = basePos + tryDir;

                // ❌ Không cho phòng nào xuất hiện cạnh Start trừ firstRoomPos
                bool isAdjacentToStart = directions
                    .Select(d => startPos + d)
                    .Any(p => p == nextPos && p != firstRoomPos);

                if (isAdjacentToStart)
                {
                    retry++;
                    continue;
                }

                retry++;
            } while ((dungeonMap.ContainsKey(nextPos) || CountNeighbors(nextPos) > 2) && retry < 50);

            if (retry >= 50) continue;

            dungeonMap[nextPos] = new RoomData
            {
                gridPos = nextPos,
                type = RoomType.Normal,
                roomPrefab = normalRoomPrefab
            };

            potentialDeadEnds.Add(nextPos);

            if (CountNeighbors(nextPos) > 1)
                potentialDeadEnds.Remove(basePos);
        }

        // 🔍 Gán boss & special từ dead-ends
        List<Vector2Int> deadEnds = new();
        foreach (var kv in dungeonMap)
        {
            if (kv.Value.type == RoomType.Normal && CountNeighbors(kv.Key) == 1)
                deadEnds.Add(kv.Key);
        }

        if (deadEnds.Count > 0)
        {
            Vector2Int bossPos = deadEnds[Random.Range(0, deadEnds.Count)];
            dungeonMap[bossPos].type = RoomType.Boss;
            dungeonMap[bossPos].roomPrefab = bossRoomPrefab;
            deadEnds.Remove(bossPos);
        }

        int specialRoomCount = Mathf.Min(2, deadEnds.Count);
        for (int i = 0; i < specialRoomCount; i++)
        {
            Vector2Int specialPos = deadEnds[Random.Range(0, deadEnds.Count)];
            dungeonMap[specialPos].type = RoomType.Special;
            dungeonMap[specialPos].roomPrefab = specialRoomPrefab;
            deadEnds.Remove(specialPos);
        }
    }

    void SpawnDungeon()
    {
        foreach (var kv in dungeonMap)
        {
            Vector3 worldPos = new Vector3(kv.Key.x * roomSpacing, kv.Key.y * roomSpacing, 0);
            GameObject roomGO = Instantiate(kv.Value.roomPrefab, worldPos, Quaternion.identity);
            spawnedRooms[kv.Key] = roomGO;
        }

        HashSet<string> createdCorridors = new();
        foreach (var kv in dungeonMap)
        {
            Vector2Int currentPos = kv.Key;
            Room currentRoom = spawnedRooms[currentPos].GetComponent<Room>();

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = currentPos + dir;
                if (!dungeonMap.ContainsKey(neighborPos)) continue;

                string key = currentPos.ToString() + neighborPos.ToString();
                string reverseKey = neighborPos.ToString() + currentPos.ToString();
                if (createdCorridors.Contains(key) || createdCorridors.Contains(reverseKey)) continue;

                createdCorridors.Add(key);

                Direction currentDir = ToDir(dir);
                Direction neighborDir = DirectionHelper.GetOpposite(currentDir);

                Room neighborRoom = spawnedRooms[neighborPos].GetComponent<Room>();
                currentRoom?.DisableDoor(currentDir);
                neighborRoom?.DisableDoor(neighborDir);

                Vector3 posA = new Vector3(currentPos.x, currentPos.y, 0);
                Vector3 posB = new Vector3(neighborPos.x, neighborPos.y, 0);
                Vector3 corridorPos = Vector3.Lerp(posA, posB, 0.5f) * roomSpacing;

                Quaternion rotation = (dir == Vector2Int.up || dir == Vector2Int.down)
                    ? Quaternion.Euler(0, 0, 90)
                    : Quaternion.identity;

                Instantiate(corridorPrefab, corridorPos, rotation);
            }
        }
    }

    int CountNeighbors(Vector2Int pos)
    {
        int count = 0;
        foreach (var dir in directions)
            if (dungeonMap.ContainsKey(pos + dir)) count++;
        return count;
    }

    Direction ToDir(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return Direction.Top;
        if (dir == Vector2Int.down) return Direction.Bottom;
        if (dir == Vector2Int.left) return Direction.Left;
        return Direction.Right;
    }
}