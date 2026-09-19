using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class TerrainTileData
{
    [Tooltip("The TileBase represented by this terrain definition.")]
    public TileBase tile;
    public bool isHot = false;
    public bool isCold = false;
    public bool isWalkable = false;
    public bool isSwimmable = false;
}

public class TerrainTilemap : MonoBehaviour
{
    public Tilemap tilemap;

    [Tooltip("Add one entry for each TileBase used by this tilemap.")]
    public List<TerrainTileData> tileData = new List<TerrainTileData>();

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        Pathfinder.TerrainTM = this;
    }

    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3Int cell = tilemap.WorldToCell(mouseWorld);

        TileBase tile = tilemap.GetTile(cell);
        TerrainTileData data = GetTileData(cell);

        //DebugText.text =
        //    $"Tile: {(tile ? tile.name : "None")}\n" +
        //    $"Cell: {cell.x} {cell.y}\n" +
        //    $"Walkable: {data.isWalkable}\n" +
        //    $"Swimmable: {data.isSwimmable}\n" +
        //    $"Hot: {data.isHot}\n" +
        //    $"Cold: {data.isCold}";
    }

    public TerrainTileData GetTileData(Vector3Int cell)
    {
        TileBase tile = tilemap.GetTile(cell);

        if (tile == null)
            return new TerrainTileData();

        foreach (TerrainTileData data in tileData)
        {
            if (data != null && data.tile == tile)
                return data;
        }

        return new TerrainTileData();
    }

    public void InitializeTilemap(int width = 10, int height = 10, int tile_id = 1, int water_layout = 0)
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("TerrainTilemap requires a Tilemap component.");
            return;
        }

        TileBase voidTile = GetTileById(0);
        TileBase insideTile = GetTileById(tile_id);
        TileBase waterTile = GetTileById(2);

        if (width < 0 || height < 0)
        {
            Debug.LogError($"Invalid terrain dimensions: {width}x{height}.");
            return;
        }

        if (voidTile == null)
        {
            Debug.LogError("TerrainTilemap is missing tileData[0], the border/void tile.");
            return;
        }

        if (insideTile == null)
        {
            Debug.LogError($"TerrainTilemap is missing a valid tileData[{tile_id}] interior tile.");
            return;
        }

        if ((water_layout == 1 || water_layout == 2) && waterTile == null)
        {
            Debug.LogError("TerrainTilemap is missing tileData[2], required by the selected water layout.");
            return;
        }

        tilemap.ClearAllTiles();

        for (int x = -1; x <= width; x++)
        {
            for (int y = -1; y <= height; y++)
            {
                bool isBorder = x == -1 || y == -1 || x == width || y == height;
                TileBase tile = isBorder ? voidTile : insideTile;

                if (!isBorder && water_layout == 1)
                {
                    int lowerMiddleRow = Mathf.Max(0, height / 2 - 1);
                    int upperMiddleRow = Mathf.Min(height - 1, lowerMiddleRow + 1);
                    bool inWaterLine = x < width * 0.75f &&
                                       (y == lowerMiddleRow || y == upperMiddleRow);

                    if (inWaterLine)
                        tile = waterTile;
                }
                else if (!isBorder && water_layout == 2)
                {
                    float middleX = (width - 1) * 0.5f;
                    float middleY = (height - 1) * 0.5f;
                    float distanceFromMiddle = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(middleX, middleY));

                    if (distanceFromMiddle > width * 0.2f)
                        tile = waterTile;
                }

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    public void ClearTilemap()
    {
        tilemap.ClearAllTiles();
    }

    private TileBase GetTileById(int id)
    {
        if (id < 0 || id >= tileData.Count || tileData[id] == null)
            return null;

        return tileData[id].tile;
    }

    public Vector3 GetWorldPosition(Vector3Int cell)
    {
        return tilemap.GetCellCenterWorld(cell);
    }
}
