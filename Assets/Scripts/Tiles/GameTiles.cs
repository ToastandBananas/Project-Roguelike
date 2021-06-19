using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameTiles : MonoBehaviour
{
    public static GameTiles instance;

    public Tilemap groundTilemap;
    public Tilemap shallowWaterTilemap;
    //public Tilemap wallsTilemap;
    //public Tilemap obstaclesTilemap;
    //public Tilemap roadTilemap;
    //public Tilemap closedDoorsTilemap;

    public Dictionary<Vector3, Tile> groundTiles       = new Dictionary<Vector3, Tile>();
    public Dictionary<Vector3, Tile> shallowWaterTiles = new Dictionary<Vector3, Tile>();
    //public Dictionary<Vector3, Tile> wallTiles       = new Dictionary<Vector3, Tile>();
    //public Dictionary<Vector3, Tile> obstacleTiles   = new Dictionary<Vector3, Tile>();
    //public Dictionary<Vector3, Tile> roadTiles       = new Dictionary<Vector3, Tile>();
    //public Dictionary<Vector3, Tile> closedDoorTiles = new Dictionary<Vector3, Tile>();

    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of GameTiles. Fix me!");
                Destroy(this);
            }
        }
        else
            instance = this;
        #endregion
        
        GetWorldTiles();
    }

    void Start()
    {
        var gg = AstarPath.active.data.gridGraph;
        for (int y = 0; y < gg.depth; y++)
        {
            for (int x = 0; x < gg.width; x++)
            {
                var node = gg.GetNode(x, y);
                if (GetTileFromWorldPosition((Vector3)node.position - new Vector3(0.5f, 0.5f), shallowWaterTiles) != null)
                    node.Tag = 2;
                else
                    node.Tag = 0;
            }
        }
    }

    // Use this for initialization
    void GetWorldTiles()
    {
        // Ground Tiles:
        GetTilesFromTilemap(groundTilemap, groundTiles);

        // Shallow Water Tiles:
        GetTilesFromTilemap(shallowWaterTilemap, shallowWaterTiles);

        // Wall Tiles:
        //GetTilesFromTilemap(wallsTilemap, wallTiles);

        // Obstacle Tiles:
        //GetTilesFromTilemap(obstaclesTilemap, obstacleTiles);

        // Road Tiles:
        //GetTilesFromTilemap(roadTilemap, roadTiles);

        // Closed Door Tiles:
        //GetTilesFromTilemap(closedDoorsTilemap, closedDoorTiles);
    }

    public static Tile GetTileFromWorldPosition(Vector3 worldPos, Dictionary<Vector3, Tile> tileDictionary)
    {
        foreach (Tile tile in tileDictionary.Values)
        {
            if (tile.WorldLocation == worldPos)
                return tile;
        }

        return null;
    }

    private void GetTilesFromTilemap(Tilemap tilemap, Dictionary<Vector3, Tile> tileDictionary)
    {
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            // The current world position we are at in our loop
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            // Check if our current position has a tile
            if (tilemap.HasTile(localPlace) == false) continue;

            // If there is a tile, set the tile data
            Tile tile = new Tile
            {
                LocalPlace = localPlace,
                WorldLocation = tilemap.CellToWorld(localPlace),
                TileBase = tilemap.GetTile(localPlace),
                TilemapMember = tilemap,
                Name = localPlace.x + "," + localPlace.y
            };

            // Then store the tile in our tiles dictionary
            tileDictionary.Add(tile.WorldLocation, tile);
        }
    }

    public TileBase GetCell(Tilemap tilemap, Vector2 cellWorldPos)
    {
        return tilemap.GetTile(tilemap.WorldToCell(cellWorldPos));
    }

    public bool HasTile(Tilemap tilemap, Vector2 cellWorldPos)
    {
        return tilemap.HasTile(tilemap.WorldToCell(cellWorldPos));
    }
}
