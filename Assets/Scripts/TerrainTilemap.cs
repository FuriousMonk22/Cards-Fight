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
    Tilemap tilemap;
    TMP_Text debugText;

    Dictionary<string, TerrainTileData> tile_data = new Dictionary<string, TerrainTileData>();

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        debugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<TMP_Text>();

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

        debugText.text =
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
}