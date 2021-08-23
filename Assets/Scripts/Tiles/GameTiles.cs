using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

public class GameTiles : MonoBehaviour
{
    public static GameTiles instance;

    public Tilemap groundTilemap;
    public Tilemap shallowWaterTilemap;
    //public Tilemap wallsTilemap;
    //public Tilemap obstaclesTilemap;
    //public Tilemap roadTilemap;
    //public Tilemap closedDoorsTilemap;

    public static Dictionary<Vector2, Tile> groundTiles       = new Dictionary<Vector2, Tile>();
    public static Dictionary<Vector2, Tile> shallowWaterTiles = new Dictionary<Vector2, Tile>();
    //public Dictionary<Vector2, Tile> wallTiles       = new Dictionary<Vector2, Tile>();
    //public Dictionary<Vector2, Tile> obstacleTiles   = new Dictionary<Vector2, Tile>();
    //public Dictionary<Vector2, Tile> roadTiles       = new Dictionary<Vector2, Tile>();
    //public Dictionary<Vector2, Tile> closedDoorTiles = new Dictionary<Vector2, Tile>();

    public static Dictionary<Vector2, CharacterManager> characters = new Dictionary<Vector2, CharacterManager>();
    public static Dictionary<Vector2, List<CharacterManager>> deadCharacters = new Dictionary<Vector2, List<CharacterManager>>();
    public static Dictionary<Vector2, List<ItemData>> itemDatas = new Dictionary<Vector2, List<ItemData>>();
    public static Dictionary<Vector2, GameObject> objects = new Dictionary<Vector2, GameObject>();

    [HideInInspector] public GridGraph gridGraph;

    Vector3 gridOffset = new Vector3(0.5f, 0.5f);

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
        // Update tags of each node
        gridGraph = AstarPath.active.data.gridGraph;
        for (int y = 0; y < gridGraph.depth; y++)
        {
            for (int x = 0; x < gridGraph.width; x++)
            {
                var node = gridGraph.GetNode(x, y);
                SetTagForNode(node);
            }
        }
    }

    public void SetTagForNode(GraphNode node)
    {
        if (GetTileFromWorldPosition((Vector3)node.position - gridOffset, shallowWaterTiles) != null)
            node.Tag = 2;
        else
            node.Tag = 0;
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

    public static Tile GetTileFromWorldPosition(Vector2 worldPos, Dictionary<Vector2, Tile> tileDictionary)
    {
        tileDictionary.TryGetValue(worldPos, out Tile tile);
        //foreach (Tile tile in tileDictionary.Values)
        //{
            //if (tile.WorldLocation == worldPos)
                //return tile;
        //}

        return tile;
    }

    void GetTilesFromTilemap(Tilemap tilemap, Dictionary<Vector2, Tile> tileDictionary)
    {
        foreach (Vector2Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            // The current world position we are at in our loop
            Vector2Int localPlace = new Vector2Int(pos.x, pos.y);

            // Check if our current position has a tile
            if (tilemap.HasTile((Vector3Int)localPlace) == false) continue;

            // If there is a tile, set the tile data
            Tile tile = new Tile
            {
                LocalPlace = localPlace,
                WorldLocation = Utilities.ClampedPosition(tilemap.CellToWorld((Vector3Int)localPlace) + new Vector3(0.5f, 0.5f)),
                TileBase = tilemap.GetTile((Vector3Int)localPlace),
                MyTilemap = tilemap,
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

    public static void AddCharacter(CharacterManager characterManager, Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        if (characters.ContainsKey(position) == false)
            characters.Add(position, characterManager);
    }

    public static void RemoveCharacter(Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        characters.Remove(position);
    }

    public static void AddObject(GameObject gameObject, Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        objects.Add(position, gameObject);
    }

    public static void RemoveObject(Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        objects.Remove(position);
    }

    public static void AddDeadCharacter(CharacterManager character, Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        if (deadCharacters.ContainsKey(position) == false)
        {
            deadCharacters.Add(position, new List<CharacterManager>());
            deadCharacters.TryGetValue(position, out List<CharacterManager> list);
            if (list.Contains(character) == false)
                list.Add(character);
        }
        else
        {
            deadCharacters.TryGetValue(position, out List<CharacterManager> list);
            if (list.Contains(character) == false)
                list.Add(character);
        }
    }

    public static void RemoveDeadCharacter(CharacterManager character, Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        if (itemDatas.ContainsKey(position))
        {
            deadCharacters.TryGetValue(position, out List<CharacterManager> list);
            list.Remove(character);
        }
    }

    public static void AddItemData(ItemData itemData, Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        if (itemDatas.ContainsKey(position) == false)
        {
            itemDatas.Add(position, new List<ItemData>());
            itemDatas.TryGetValue(position, out List<ItemData> list);
            if (list.Contains(itemData) == false)
                list.Add(itemData);
        }
        else
        {
            itemDatas.TryGetValue(position, out List<ItemData> list);
            if (list.Contains(itemData) == false)
                list.Add(itemData);
        }
    }

    public static void RemoveItemData(ItemData itemData, Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        if (itemDatas.ContainsKey(position))
        {
            itemDatas.TryGetValue(position, out List<ItemData> list);
            list.Remove(itemData);
        }
    }
}
