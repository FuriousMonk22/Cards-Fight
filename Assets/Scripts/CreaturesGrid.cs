using System;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;

// Manages the spawning and position of creatures in the grid, also has utility functions

public class CreaturesGrid : MonoBehaviour
{
    private const string CreaturePrefabPath = "Creatures/";

    TerrainTilemap TerrainTM; // reference to terrain, used for determining if creature can be placed/swapped to spot

    public int Width;
    public int Height;
    public GameObject[,] Creatures;

    private bool dragging;
    private Vector2Int dragStart;

    private void Awake()
    {
        TerrainTM = GameObject.FindWithTag("TerrainTilemap").GetComponent<TerrainTilemap>();
        Pathfinder.creaturesGrid = this;
    }

    private void Update()
    {
        ManageMouseDrag();
    }

    public void ManageMouseDrag()
    {
        if (!GamePhaseManager.Instance.CanPlaceCreatures)
        {
            dragging = false;
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();

            Vector3 world = Camera.main.ScreenToWorldPoint(
                new Vector3(mouse.x, mouse.y, -Camera.main.transform.position.z)
            );

            Vector3Int cell = TerrainTM.tilemap.WorldToCell(world);

            if (IsInsideBounds(cell.x, cell.y) && Creatures[cell.x, cell.y] != null)
            {
                dragging = true;
                dragStart = new Vector2Int(cell.x, cell.y);
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && dragging)
        {
            dragging = false;

            Vector2 mouse = Mouse.current.position.ReadValue();

            Vector3 world = Camera.main.ScreenToWorldPoint(
                new Vector3(mouse.x, mouse.y, -Camera.main.transform.position.z)
            );

            Vector3Int cell = TerrainTM.tilemap.WorldToCell(world);

            if (IsInsideBounds(cell.x, cell.y))
            {
                Swap(
                    dragStart.x,
                    dragStart.y,
                    cell.x,
                    cell.y
                );
            }
        }
    }

    public void InitializeGrid(int width, int height)
    {
        Width = width;
        Height = height;
        Creatures = new GameObject[width, height];
    }

    public void ClearGrid()
    {
        if (Creatures == null)
            return;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Creatures[x, y] != null)
                    Destroy(Creatures[x, y]);

                Creatures[x, y] = null;
            }
        }
    }

    public bool IsOccupied(int x, int y)
    {
        if (!IsInsideBounds(x, y) || Creatures[x, y] != null)
            return true; // Outside the grid = cannot be placed
        return false;
    }

    public int getCreatureCount(int team)
    {
        int total = 0;
        foreach (GameObject go in Creatures)
            if (go != null && go.GetComponent<Creature>().team == team) total++;
        return total;
    }

    // swaps positions (usually between a creature and an empty tile)
    // can be forced even if IsInCombat by moving creatures
    public void Swap(int x1, int y1, int x2, int y2, bool force = false)
    {
        if (!GamePhaseManager.Instance.CanPlaceCreatures && !force)
        {
            Debug.Log("Cannot swap creatures during combat.");
            return;
        }

        if (!IsInsideBounds(x1, y1) || !IsInsideBounds(x2, y2))
        {
            Debug.Log("Failed to swap creatures, outside of bounds.");
            return;
        }

        GameObject temp = Creatures[x1, y1];

        Creatures[x1, y1] = Creatures[x2, y2];
        Creatures[x2, y2] = temp;

        if (Creatures[x1, y1] != null)
        {
            Creatures[x1, y1].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x1, y1, 0));
            Creatures[x1, y1].GetComponent<Creature>().cell = new Vector3Int(x1, y1, 0);
        }

        if (Creatures[x2, y2] != null)
        {
            Creatures[x2, y2].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x2, y2, 0));
            Creatures[x2, y2].GetComponent<Creature>().cell = new Vector3Int(x2, y2, 0);
        }

        Debug.Log($"Swapped ({x1}, {y1}) with ({x2}, {y2})");
    }

    public bool Spawn(string creatureName, int x, int y)
    {
        if (!GamePhaseManager.Instance.CanPlaceCreatures)
            return false;

        if (Creatures == null || IsOccupied(x, y))
            return false;


        CreatureData data = CreatureData.Load(creatureName);

        if (data == null)
        {
            Debug.LogError($"CreatureData '{creatureName}' not found.");
            return false;
        }

        // create Creature and allign to tilemap
        GameObject go = new GameObject(creatureName);
        go.transform.SetParent(transform);

        Tilemap tm = TerrainTM.GetComponent<Tilemap>();
        Vector3Int spawnCell = new Vector3Int(x, y, 0);
        go.transform.position = tm.GetCellCenterWorld(spawnCell);

        Creature creature = go.AddComponent<Creature>();

        creature.creaturesGrid = this;
        creature.cell = new Vector3Int(x, y, 0);
        creature.terrainTilemap = TerrainTM;

        creature.team = y < Height / 2.0 ? 1 : 0;

        creature.Initialize(data);

        if (creature.TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
        {
            renderer.color = creature.team == 1
                ? new Color(1f, 0.3f, 0.4f)
                : new Color(0.3f, 0.3f, 1f);
        }

        Creatures[x, y] = go;

        return true;
    }

    public void RemoveCreature(int x, int y)
    {
        if (!IsInsideBounds(x, y)) return;

        GameObject creature = Creatures[x, y];

        if (creature == null)
            return;
        
        Creatures[x, y] = null;
        Destroy(creature);
    }

    public bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
