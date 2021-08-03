using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile
{
    public Vector2Int LocalPlace { get; set; }

    public Vector2 WorldLocation { get; set; }

    public TileBase TileBase { get; set; }

    public Tilemap MyTilemap { get; set; }

    public string Name { get; set; }
}
