using System;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CreaturesGrid : MonoBehaviour
{
    private const string CreaturePrefabPath = "Creatures/";

    TerrainTilemap TerrainTM;

    public int Width = 3;
    public int Height = 3;
    public float CellSize = 1f;

    public float tick_timer = 0f;
    private float tick_length = 0.5f;

    public GameObject[,] Creatures;
    private bool dragging;
    private Vector2Int dragStart;
    private Timer ConsoleTimer;

    private void Awake()
    {
        ConsoleTimer = GameObject.FindGameObjectWithTag("GameConsole").GetComponent<Timer>();
        TerrainTM = GameObject.FindWithTag("TerrainTilemap").GetComponent<TerrainTilemap>();
        
        Pathfinder.creaturesGrid = this;

        InitializeGrid(10, 8);
    }

    void Start()
    {
        InitializeGrid(10, 8);
    }

    private void Update()
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

    public bool IsOccupied(int x, int y)
    {
        if (!IsInsideBounds(x, y) || Creatures[x, y] != null)
            return true; // Outside the grid = cannot be placed

        return Creatures[x, y] != null;
    }

    public int getCreatureCount(int team)
    {
        int total = 0;
        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Height; j++)
                if (Creatures[i, j] != null && Creatures[i, j].GetComponent<Creature>().team == team) total++;
        return total;
    }

    public void Swap(int x1, int y1, int x2, int y2)
    {
        if (!GamePhaseManager.Instance.CanPlaceCreatures)
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

        Vector3Int spawnCell = new Vector3Int(x, y, 0);

        GameObject go = new GameObject(creatureName);
        go.transform.SetParent(transform);
        go.transform.position =
            GridManager.Instance.GroundTilemap.GetCellCenterWorld(spawnCell);

        Creature creature = go.AddComponent<Creature>();
        creature.Initialize(data);
        creature.cell = new Vector3Int(x, y, 0);
        creature.creaturesGrid = this;

        creature.team = y < Height / 2.0 ? 1 : 0;

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