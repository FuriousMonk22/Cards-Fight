using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class TerrainTileData
{
    public bool isHot = false;
    public bool isCold = false;
    public bool isWalkable = false;
    public bool isSwimmable = false;
}

public class TerrainTilemap : MonoBehaviour
{
    public Tilemap tilemap;
    TMP_Text DebugText;

    Dictionary<string, TerrainTileData> tile_data = new Dictionary<string, TerrainTileData>();

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    void Start()
    {
        DebugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<TMP_Text>();

        tile_data.Add("sand", new TerrainTileData());
        tile_data["sand"].isHot = true;


        tile_data.Add("water", new TerrainTileData());
        tile_data["water"].isSwimmable = true;

        tile_data.Add("snow", new TerrainTileData());
        tile_data["snow"].isCold = true;


        tile_data.Add("mountain", new TerrainTileData());
        tile_data["mountain"].isCold = true;
    }

    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3Int cell = tilemap.WorldToCell(mouseWorld);

        TileBase tile = tilemap.GetTile(cell);

        TerrainTileData data = GetTileData(tile ? tile.name : "");

        DebugText.text =
            $"Tile: {(tile ? tile.name : "None")}\n" +
            $"Cell: {cell.x} {cell.y}\n" +
            $"Walkable: {data.isWalkable}\n" +
            $"Swimmable: {data.isSwimmable}\n" +
            $"Hot: {data.isHot}\n" +
            $"Cold: {data.isCold}";
    }

    TerrainTileData GetTileData(string tileName)
    {
        if (tile_data.ContainsKey(tileName))
            return tile_data[tileName];

        return new TerrainTileData();
    }

    public Vector3 GetWorldPosition(Vector3Int cell)
    {
        return tilemap.GetCellCenterWorld(cell);
    }
}