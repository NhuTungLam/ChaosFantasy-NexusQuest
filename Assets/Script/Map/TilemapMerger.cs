using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapMerger : MonoBehaviour
{
    public static TilemapMerger Instance ;
    public Tilemap bigGroundTilemap;
    public Tilemap bigWallTilemap;
    private void Awake()
    {
        Instance = this;  
    }
    public void MergePrefabTilesIntoBigTilemap(GameObject instance)
    {
        Tilemap prefabGround = instance.transform.Find("ground").GetComponent<Tilemap>();
        Tilemap prefabWall = instance.transform.Find("wall").GetComponent<Tilemap>();
        
        CopyTiles(prefabGround, bigGroundTilemap);
        CopyTiles(prefabWall, bigWallTilemap);
        prefabGround.gameObject.SetActive(false);
        prefabWall.gameObject.SetActive(false);
    }

    private void CopyTiles(Tilemap source, Tilemap destination)
    {
        BoundsInt bounds = source.cellBounds;

        foreach (Vector3Int localPos in bounds.allPositionsWithin)
        {
            TileBase tile = source.GetTile(localPos);
            if (tile != null)
            {
                // Convert source local cell position to world position
                Vector3 worldPos = source.CellToWorld(localPos);

                // Then convert that world position into the destination cell position
                Vector3Int destPos = destination.WorldToCell(worldPos);

                destination.SetTile(destPos, tile);
                destination.SetTransformMatrix(destPos, source.GetTransformMatrix(localPos));
            }
        }
    }
}