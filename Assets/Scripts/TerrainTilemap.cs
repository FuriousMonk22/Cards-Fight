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
        DebugText = GameObject.FindGameObjectWithTag("DebugText")
            .GetComponent<TMP_Text>();

        // FIELD
        tile_data.Add("field", new TerrainTileData
        {
            isWalkable = true,
            isSwimmable = false
        });

        // FOREST
        tile_data.Add("forest", new TerrainTileData
        {
            isWalkable = true,
            isSwimmable = false
        });

        // MOUNTAIN
        // Perete pentru walk/swim.
        // Flying poate trece peste el.
        tile_data.Add("mountain", new TerrainTileData
        {
            isWalkable = false,
            isSwimmable = false,
            isCold = true
        });

        // SAND
        tile_data.Add("sand", new TerrainTileData
        {
            isWalkable = true,
            isSwimmable = false,
            //isHot = true
        });

        // SNOW
        tile_data.Add("snow", new TerrainTileData
        {
            isWalkable = true,
            isSwimmable = false,
            //isCold = true
        });

        // WATER
        tile_data.Add("water", new TerrainTileData
        {
            isWalkable = false,
            isSwimmable = true
        });

        // VOID
        // Nimeni care merge sau înoată nu poate intra.
        tile_data.Add("void", new TerrainTileData
        {
            isWalkable = false,
            isSwimmable = false
        });

        // NEW TILE
        // Până când îi dai un rol clar, îl tratăm ca perete.
        tile_data.Add("New Tile", new TerrainTileData
        {
            isWalkable = false,
            isSwimmable = false
        });
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

    public bool CanCreatureTraverse(
    Vector3Int cell,
    CreatureData creature)
    {
        if (creature == null)
            return false;

        TileBase tile = tilemap.GetTile(cell);

        if (tile == null)
            return false;

        TerrainTileData data =
            GetTileData(tile.name);

        // VOID = perete absolut.
        // Nici măcar flying nu poate trece.
        if (tile.name == "void")
            return false;

        // Flying poate trece peste orice alt teren.
        if (creature.canFly)
            return true;

        if (creature.canWalk &&
            data.isWalkable)
        {
            return true;
        }

        if (creature.canSwim &&
            data.isSwimmable)
        {
            return true;
        }

        return false;
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