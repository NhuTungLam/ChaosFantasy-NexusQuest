using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    void Start()
    {
        GenerateDungeon();
    }

    public GameObject[] roomPrefabs;
    public GameObject[] bossRooms;
    public GameObject[] specialRooms;
    public GameObject[] corridorPrefabs;

    public int dungeonSize = 10;
    public int maxComplexRooms = 3;
    public int maxSpecialRoom = 2;
    public Vector2Int roomSize = new Vector2Int(10, 10);

    private Dictionary<Vector2Int, (Room room, Direction dir, int depth)> spawnedRooms = new();
    private Vector2Int lastSpawnedRoom;
    private Dictionary<Vector2Int, int> roomDepths = new();
    private List<Vector2Int> deadEnds = new();
    private int currentComplexRoomCount = 0;

    [ContextMenu("gen")]
    public void GenerateDungeon()
    {
        currentComplexRoomCount = 0;
        lastSpawnedRoom = Vector2Int.zero;
        spawnedRooms.Clear();
        roomDepths.Clear();
        deadEnds.Clear();

        Vector2Int startPos = Vector2Int.zero;
        GenerateRoom(startPos, Direction.Bottom, 0, 2);
        while (spawnedRooms.Count < dungeonSize)
        {
            var pair = spawnedRooms[lastSpawnedRoom];
            spawnedRooms.Remove(lastSpawnedRoom);
            roomDepths.Remove(lastSpawnedRoom);
            deadEnds.Remove(lastSpawnedRoom);
            Destroy(pair.room.gameObject);

            GenerateRoom(lastSpawnedRoom, pair.dir, pair.depth, RoomConCount(true));
        }
        PlaceSpecialRooms();
        foreach( var pair in spawnedRooms.Values)
        {
            pair.room.getActiveDoor();
        }
    }

    public int RoomConCount(bool reroll = false)
    {
        if (reroll)
        {
            return Random.Range(0, 4) == 0 ? 3 : 2;
        }

        int count = spawnedRooms.Count;
        if (count >= dungeonSize)
            return 1;

        int weight1 = count;
        int weight2 = weight1 / 2;
        int weight3 = (currentComplexRoomCount < maxComplexRooms) ? 3 : 0;
        int weight4 = (currentComplexRoomCount < maxComplexRooms) ? 1 : 0;

        List<int> choices = new();
        for (int i = 0; i < weight1; i++) choices.Add(1);
        for (int i = 0; i < weight2; i++) choices.Add(2);
        for (int i = 0; i < weight3; i++) choices.Add(3);
        for (int i = 0; i < weight4; i++) choices.Add(4);

        int selected = choices[Random.Range(0, choices.Count)];
        if (selected >= 3) currentComplexRoomCount++;
        return selected;
    }

    private bool GenerateRoom(Vector2Int position, Direction fromDir, int depth, int conCount)
    {
        if (spawnedRooms.ContainsKey(position))
            return false;

        Room fromRoom = null;
        Vector2Int previousPos = position - DirectionHelper.ToVector2Int(fromDir);
        if (spawnedRooms.TryGetValue(previousPos, out var previous))
        {
            fromRoom = previous.room;
        }

        GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        GameObject roomGO = Instantiate(prefab, (Vector2)position * roomSize, Quaternion.identity);
        roomGO.name = "" + spawnedRooms.Count;
        Room room = roomGO.GetComponent<Room>();
        spawnedRooms[position] = (room, fromDir, depth);
        roomDepths[position] = depth;
        lastSpawnedRoom = position;

        if (fromRoom != null)
        {
            room.DisableDoor(DirectionHelper.GetOpposite(fromDir));
            fromRoom.DisableDoor(fromDir);
            SpawnCorridor(position, previousPos, fromDir);
        }
        else
        {
            room.EnableDoor(DirectionHelper.GetOpposite(fromDir));
        }

        List<Direction> directions = new List<Direction>((Direction[])System.Enum.GetValues(typeof(Direction)));
        Shuffle(directions);
        directions.Remove(DirectionHelper.GetOpposite(fromDir));

        for (int i = 0; i < directions.Count; i++)
        {
            int targetCon = RoomConCount();
            if (i < conCount - 1)
            {
                Vector2Int nextPos = position + DirectionHelper.ToVector2Int(directions[i]);
                if (!GenerateRoom(nextPos, directions[i], depth + 1, targetCon))
                    room.DisableDoor(directions[i]);
            }
            else
            {
                room.EnableDoor(directions[i]);
            }
        }

        if (conCount == 1)
            deadEnds.Add(position);

        return true;
    }
    
    private void SpawnCorridor(Vector2Int from, Vector2Int to, Direction dir)
    {
        Vector2 mid = ((Vector2)(from + to)) / 2f;
        Vector3 worldPos = (Vector2)mid * roomSize;

        Quaternion rotation = Quaternion.identity;
        if (dir == Direction.Top || dir == Direction.Bottom)
            rotation = Quaternion.Euler(0, 0, 90);

        if (corridorPrefabs != null && corridorPrefabs.Length > 0)
        {
            GameObject selected = corridorPrefabs[Random.Range(0, corridorPrefabs.Length)];
            Instantiate(selected, worldPos, rotation);
        }
    }

    private void PlaceSpecialRooms()
    {
        if (deadEnds.Count == 0) return;
        var orderedDeadEnds = deadEnds.OrderBy(pos => roomDepths[pos]).ToList();
        Vector2Int bossPos = orderedDeadEnds[^1];
        ReplaceRoomWith(bossRooms, bossPos);

        for (int i = 0; i < Mathf.Min(maxSpecialRoom, orderedDeadEnds.Count - 1); i++)
        {
            ReplaceRoomWith(specialRooms, orderedDeadEnds[i]);
        }
    }

    private void ReplaceRoomWith(GameObject[] roomOptions, Vector2Int position)
    {
        if (!spawnedRooms.ContainsKey(position)) return;

        var oldRoom = spawnedRooms[position];
        Vector3 pos = oldRoom.room.transform.position;
        Quaternion rot = oldRoom.room.transform.rotation;
        Destroy(oldRoom.room.gameObject);

        GameObject selected = roomOptions[Random.Range(0, roomOptions.Length)];
        GameObject newRoomGO = Instantiate(selected, pos, rot);
        Room newRoom = newRoomGO.GetComponent<Room>();
        spawnedRooms[position] = (newRoom, oldRoom.dir, oldRoom.depth);

        foreach (Direction dir in (Direction[])System.Enum.GetValues(typeof(Direction)))
        {
            if (dir == DirectionHelper.GetOpposite(oldRoom.dir))
                newRoom.DisableDoor(dir);
            else
                newRoom.EnableDoor(dir);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}