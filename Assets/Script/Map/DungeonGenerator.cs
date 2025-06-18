using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using Photon.Pun;
public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance;
    public void Awake()
    {
        Instance = this;
    }
    public int stageLevel;
    public GameObject[] roomPrefabs;
    public GameObject[] bossRooms;
    public GameObject[] specialRooms;
    public int dungeonSize = 10;
    public int maxComplexRooms = 3;
    public int maxSpecialRoom = 2;
    public Vector2Int roomSize = new Vector2Int(10, 10);

    public Tilemap corridorTilemap;
    public Tilemap wallTilemap;

    public TileBase corridorTile;
    public TileBase wallTileRight;
    public TileBase wallTileLeft;
    public TileBase wallTileTop;
    public TileBase wallTileBottom;

    private Dictionary<Vector2Int, (Room room, Direction dir, int depth)> spawnedRooms = new();
    private Vector2Int lastSpawnedRoom;
    private Dictionary<Vector2Int, int> roomDepths = new();
    private List<Vector2Int> deadEnds = new();
    private HashSet<Vector3Int> corridorPositionsVertical = new();
    private HashSet<Vector3Int> corridorPositionsHorizontal = new();
    private static readonly List<Vector3Int> directions8 = new()
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(1, 1, 0), new Vector3Int(-1, 1, 0),
        new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0)
    };

    private int currentComplexRoomCount = 0;

    public void Start()
    {
        stageLevel = 1;
    }

    [ContextMenu("gen")]
    public void GenerateDungeon()
    {
        var allMonoBehaviours = GameObject.FindObjectsOfType<MonoBehaviour>();

        var allInteractables = allMonoBehaviours
            .Where(m => m is IInteractable)
            .ToList(); 

        foreach ( var interactable in allInteractables)
        {
            if ((interactable as IInteractable).CanInteract())
            Destroy(interactable.gameObject);
        }
        currentComplexRoomCount = 0;
        lastSpawnedRoom = Vector2Int.zero;
        foreach (var room in spawnedRooms.Values)
        {
            Destroy(room.room.gameObject);
            
        }
        spawnedRooms.Clear();
        roomDepths.Clear();
        deadEnds.Clear();
        corridorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        corridorPositionsVertical.Clear();
        corridorPositionsHorizontal.Clear();

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

        MergeAllRoomTilemaps();
        GenerateAllCorridors();
        GenerateCorridorWalls();
    }
    public Dictionary<(int x,int y), (Direction dir, string prefabName)> GetSpawnedRooms()
    {
        Dictionary<(int x,int y), (Direction dir, string prefabName)> layout = new();
        foreach(var v in spawnedRooms)
        {
            layout[(v.Key.x,v.Key.y)] = (v.Value.dir, v.Value.room.roomname);
            
        }
        return layout;
    }


    public int RoomConCount(bool reroll = false)
    {
        if (reroll)
        {
            return Random.Range(0, 4) == 0 ? 3 : 2;
        }

        int count = spawnedRooms.Count;
        if (count >= dungeonSize) return 1;

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
        if (spawnedRooms.ContainsKey(position)) return false;

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
        }
        else
        {
            room.EnableDoor(DirectionHelper.GetOpposite(fromDir));
        }

        List<Direction> directions = new((Direction[])System.Enum.GetValues(typeof(Direction)));
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

    private void MergeAllRoomTilemaps()
    {
        foreach (var kvp in spawnedRooms)
        {
            TilemapMerger.Instance.MergePrefabTilesIntoBigTilemap(kvp.Value.room.gameObject);
        }
    }

    private void GenerateAllCorridors()
    {
        foreach (var kvp in spawnedRooms)
        {
            Vector2Int pos = kvp.Key;
            Direction dir = DirectionHelper.GetOpposite(kvp.Value.dir);

            Vector2Int neighborPos = pos + DirectionHelper.ToVector2Int(dir);
            if (spawnedRooms.ContainsKey(neighborPos))
            {
                SpawnCorridor(pos, neighborPos, dir);
            }
        }
    }

    private void SpawnCorridor(Vector2Int from, Vector2Int to, Direction dir)
    {
        bool vertical = from.x == to.x;
        Vector2Int start = from * roomSize;
        Vector2Int end = to * roomSize;
        Vector2Int current = start;

        while (current.x != end.x)
        {
            SetCorridorTile(current, vertical);
            current.x += end.x > current.x ? 1 : -1;
        }

        while (current.y != end.y)
        {
            SetCorridorTile(current, vertical);
            current.y += end.y > current.y ? 1 : -1;
        }

        SetCorridorTile(current, vertical);
    }

    private void SetCorridorTile(Vector2Int position, bool vertical)
    {
        for (int x = -2; x <= 1; x++)
        {
            for (int y = -2; y <= 1; y++)
            {
                Vector3Int offset = new(position.x + x, position.y + y, 0);
                corridorTilemap.SetTile(offset, corridorTile);
                if (vertical) corridorPositionsVertical.Add(offset);
                else corridorPositionsHorizontal.Add(offset);
            }
        }
    }

    private void GenerateCorridorWalls()
    {
        foreach (var pos in corridorPositionsVertical)
        {
            foreach (var dir in directions8)
            {
                Vector3Int neighbor = pos + dir;
                if (!corridorPositionsVertical.Contains(neighbor) && corridorTilemap.GetTile(neighbor) == null)
                {
                    TileBase tileToSet = dir.x > 0 ? wallTileRight : wallTileLeft;
                    wallTilemap.SetTile(neighbor, tileToSet);
                }
            }
        }

        foreach (var pos in corridorPositionsHorizontal)
        {
            foreach (var dir in directions8)
            {
                Vector3Int neighbor = pos + dir;
                if (!corridorPositionsHorizontal.Contains(neighbor) && corridorTilemap.GetTile(neighbor) == null)
                {
                    TileBase tileToSet = dir.y > 0 ? wallTileTop : wallTileBottom;
                    wallTilemap.SetTile(neighbor, tileToSet);
                }
            }
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

    public static Vector2Int ToVector2Int(Vector3 input)
    {
        return Vector2Int.RoundToInt(input - new Vector3(0.5f, 0.5f));
    }
   
    public string SaveLayout()
    {
        Dictionary<Vector2Int, (Direction dir, string prefabName)> data = new();
        foreach (var kvp in spawnedRooms)
        {
            Vector2Int pos = kvp.Key;
            Direction dir = kvp.Value.dir;
            string prefabName = kvp.Value.room.roomname;
            data[pos] = (dir, prefabName);
        }

        string json = JsonUtility.ToJson(new SerializableRoomLayout(data));
        return json;
    }

    [ContextMenu("LoadLayout")]
    public void LoadLayout(string json)
    {
        
        var layout = JsonUtility.FromJson<SerializableRoomLayout>(json);
        Dictionary<Vector2Int, (Direction, string)> loaded = layout.ToDictionary();

        GenerateDungeonFromLayout(loaded);
    }
    
    public void GenerateDungeonFromLayout(Dictionary<Vector2Int, (Direction dir, string prefabName)> layout)
    {
        ClearExistingDungeon();

        foreach (var kvp in layout)
        {
            Vector2Int pos = kvp.Key;
            Direction dir = kvp.Value.dir;
            string prefabName = kvp.Value.prefabName;

            GameObject prefab =     FindRoomPrefabByName(prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab '{prefabName}' not found!");
                continue;
            }

            GameObject roomGO = Instantiate(prefab, (Vector2)pos * roomSize, Quaternion.identity);
            Room room = roomGO.GetComponent<Room>();
            spawnedRooms[pos] = (room, dir, 0);
            room.name = $"Room_{pos}";
        }

        foreach (var kvp in layout)
        {
            Vector2Int pos = kvp.Key;
            Direction fromDir = kvp.Value.dir;

            Vector2Int parentPos = pos - DirectionHelper.ToVector2Int(fromDir);
            if (spawnedRooms.TryGetValue(parentPos, out var parent))
            {
                Room currentRoom = spawnedRooms[pos].room;
                Room parentRoom = parent.room;

                currentRoom.DisableDoor(DirectionHelper.GetOpposite(fromDir));
                parentRoom.DisableDoor(fromDir);
            }
        }

        MergeAllRoomTilemaps();
        GenerateAllCorridors();
        GenerateCorridorWalls();
    }

    public GameObject FindRoomPrefabByName(string prefabName)
    {
        return roomPrefabs.Concat(specialRooms).Concat(bossRooms)
            .FirstOrDefault(p => p.TryGetComponent(out Room r) && r.roomname == prefabName);
    }

    private void ClearExistingDungeon()
    {
        foreach (var kvp in spawnedRooms)
        {
            if (kvp.Value.room != null)
                Destroy(kvp.Value.room.gameObject);
        }

        spawnedRooms.Clear();
        roomDepths.Clear();
        deadEnds.Clear();
        corridorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        corridorPositionsHorizontal.Clear();
        corridorPositionsVertical.Clear();
    }

    // --- Other methods (GenerateDungeon, GenerateRoom, PlaceSpecialRooms, etc.)
    // keep your original GenerateDungeon(), RoomConCount(), PlaceSpecialRooms(), ReplaceRoomWith(), MergeAllRoomTilemaps(), GenerateAllCorridors(), GenerateCorridorWalls(), SpawnCorridor(), SetCorridorTile(), Shuffle() etc.

    // Add below class to serialize Vector2Int and custom data
    [Serializable]
    public class SerializableRoomLayout
    {
        public List<Entry> entries = new();

        [Serializable]
        public struct Entry
        {
            public int x, y;
            public Direction dir;
            public string prefabName;

            public Entry(Vector2Int pos, Direction dir, string prefabName)
            {
                this.x = pos.x;
                this.y = pos.y;
                this.dir = dir;
                this.prefabName = prefabName;
            }

            public Vector2Int GetPosition() => new(x, y);
        }

        public SerializableRoomLayout(Dictionary<Vector2Int, (Direction dir, string prefabName)> dict)
        {
            foreach (var kvp in dict)
            {
                entries.Add(new Entry(kvp.Key, kvp.Value.dir, kvp.Value.prefabName));
            }
        }

        public Dictionary<Vector2Int, (Direction dir, string prefabName)> ToDictionary()
        {
            Dictionary<Vector2Int, (Direction, string)> dict = new();
            foreach (var entry in entries)
            {
                dict[entry.GetPosition()] = (entry.dir, entry.prefabName);
            }
            return dict;
        }
    }
    [ContextMenu("Delete Saved Dungeon Layout")]
    public void DeleteSavedLayout()
    {
        if (PlayerPrefs.HasKey("SavedDungeonLayout"))
        {
            PlayerPrefs.DeleteKey("SavedDungeonLayout");
            Debug.Log("Deleted saved dungeon layout.");
        }
        else
        {
            Debug.Log("No saved dungeon layout to delete.");
        }
    }
}
