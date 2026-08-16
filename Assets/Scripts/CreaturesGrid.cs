using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
    public GameObject[,] Reservations;

    private void Awake()
    {
        TerrainTM = GameObject.FindWithTag("TerrainTilemap")
            .GetComponent<TerrainTilemap>();

        InitializeGrid(10, 8);
    }

    void Start()
    {
        InitializeGrid(10, 8);
    }

    public void InitializeGrid(int width, int height)
    {
        Width = width;
        Height = height;

        Creatures = new GameObject[width, height];
        Reservations = new GameObject[width, height];
    }

    public bool IsOccupied(int x, int y)
    {
        if (!IsInsideBounds(x, y))
            return true; // Outside the grid = cannot be placed

        return Creatures[x, y] != null ||
            Reservations[x, y] != null;
    }


    public bool TryReserveCell(GameObject creature, Vector3Int cell)
    {
        if (!IsInsideBounds(cell.x, cell.y))
            return false;

        // E ocupată de altă creatură
        if (Creatures[cell.x, cell.y] != null &&
            Creatures[cell.x, cell.y] != creature)
        {
            return false;
        }

        // E deja rezervată de altcineva
        if (Reservations[cell.x, cell.y] != null &&
            Reservations[cell.x, cell.y] != creature)
        {
            return false;
        }

        Reservations[cell.x, cell.y] = creature;

        return true;
    }

    public void CompleteMove(
    GameObject creature,
    Vector3Int from,
    Vector3Int to)
    {
        // Eliberăm celula veche
        if (IsInsideBounds(from.x, from.y) &&
            Creatures[from.x, from.y] == creature)
        {
            Creatures[from.x, from.y] = null;
        }

        // Ocupăm celula nouă
        Creatures[to.x, to.y] = creature;

        // Scoatem rezervarea
        if (Reservations[to.x, to.y] == creature)
        {
            Reservations[to.x, to.y] = null;
        }

        UpdateCreaturesPath();
        Debug.Log($"{creature.name}: {from} -> {to}");
    }

    private void Update()
    {
        if (GamePhaseManager.Instance != null &&
        !GamePhaseManager.Instance.CanPlaceCreatures)
        {
            dragging = false;
            return;
        }
        
        Tick();

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

    public void Tick()
    {
        tick_timer -= Time.deltaTime;
        if(tick_timer < 0)
        {
            attackTick();
            removeDeadCreatures();
            tick_timer = tick_length;
        }
    }

    public void Swap(int x1, int y1, int x2, int y2)
    {
        if (GamePhaseManager.Instance != null &&
        !GamePhaseManager.Instance.CanPlaceCreatures)
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

        if (Creatures[x1, y1] != null) {
            Creatures[x1, y1].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x1, y1, 0));
            Creatures[x1, y1].GetComponent<CreatureMovement>().currentCell = new Vector3Int(x1, y1, 0);
            Creatures[x1, y1].GetComponent<CreatureMovement>().RestartPath();
        }

        if (Creatures[x2, y2] != null) {
            Creatures[x2, y2].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x2, y2, 0));
            Creatures[x2, y2].GetComponent<CreatureMovement>().currentCell = new Vector3Int(x2, y2, 0);
            Creatures[x2, y2].GetComponent<CreatureMovement>().RestartPath();
        }
        
        Debug.Log($"Swapped ({x1}, {y1}) with ({x2}, {y2})");
        UpdateCreaturesPath();
    }

    public bool Spawn(string creatureName, int x, int y)
    {
        Debug.Log(
        $"Trying Spawn {creatureName} at {x},{y}. " +
        $"Phase = {GamePhaseManager.Instance?.CurrentPhase}"
    );

        if (GamePhaseManager.Instance != null &&
            !GamePhaseManager.Instance.CanPlaceCreatures)
        {
            Debug.LogWarning(
                $"SPAWN BLOCKED! Current phase: " +
                $"{GamePhaseManager.Instance.CurrentPhase}"
            );

            return false;
        }

        if (Creatures == null)
        {
            Debug.LogError("Creatures array is NULL!");
            return false;
        }

        if (IsOccupied(x, y))
        {
            Debug.LogWarning($"Cell {x},{y} is occupied!");
            return false;
        }

        Debug.Log("Spawning" + creatureName);

        if (Creatures == null){
            Debug.Log("Failed to add creature, creature grid uninitialized.");
            return false;
        }
        if (IsOccupied(x, y)) {
            return false;
        }

        GameObject prefab = Resources.Load<GameObject>(CreaturePrefabPath + creatureName);

        if (prefab == null)
        {
            Debug.LogError($"Creature prefab '{creatureName}' not found.");
            return false;
        }

        GameObject go = Instantiate(prefab, transform);

        Vector3Int spawnCell = new Vector3Int(x, y, 0);

        Creatures[x, y] = go;

        go.transform.position =
            GridManager.Instance.GroundTilemap.GetCellCenterWorld(spawnCell);

        CreatureMovement movement =
            go.GetComponent<CreatureMovement>();

        if (movement != null)
        {
            movement.Initialize(this, spawnCell);
        }
        else
        {
            Debug.LogError("Prefab-ul nu are CreatureMovement!");
        }

        CreatureData creature =
            go.GetComponent<CreatureData>();

        // Sprite
        SpriteRenderer renderer =
            go.GetComponent<SpriteRenderer>();

        if (renderer == null)
            renderer = go.AddComponent<SpriteRenderer>();

        if (creature.Sprite != null)
        {
            renderer.sprite = creature.Sprite;
            renderer.sortingOrder = 1;
        }

        // Tint based on Y position
        if (y < Height / 2.0) {
            renderer.color = new Color(1f, 0.3f, 0.4f);
            creature.team = 1;
        }
        else {
            renderer.color = new Color(0.3f, 0.3f, 1f);
            creature.team = 0;
        }

        UpdateCreaturesPath();
        return true;
    }

    public void RemoveCreature(int x, int y)
    {
        CreatureMovement cm = Creatures[x, y].GetComponent<CreatureMovement>();
        Reservations[cm.reservedCell.x, cm.reservedCell.y] = null;
        Destroy(Creatures[x, y]);
    }

    public void UpdateCreaturesPath()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                GameObject creature = Creatures[i, j];

                if (creature == null)
                    continue;

                CreatureMovement movement =
                    creature.GetComponent<CreatureMovement>();

                if (movement == null)
                    continue;

                // Only synchronize currentCell when the creature
                // is actually represented by this grid position.
                if (!movement.IsMovingBetweenCells)
                {
                    movement.currentCell =
                        new Vector3Int(i, j, 0);
                }

                movement.enemyCell =
                    getClosestEnemyCell(i, j);

                movement.RestartPath();
            }
        }
    }

    public Vector3Int getClosestEnemyCell(int x, int y)
    {
        if (Creatures[x, y] == null) return new Vector3Int(0, 0, 0);
        int team = Creatures[x, y].GetComponent<CreatureData>().team;
        int attackRange = Creatures[x, y].GetComponent<CreatureData>().AttackRange;

        int min_dist = 999;
        Vector3Int min_pos = new Vector3Int(x, y, 0);

        for(int i = 0; i < Width; ++i)
            for(int j = 0; j < Height; ++j)
                if(Creatures[i, j] != null && Creatures[i, j].GetComponent<CreatureData>().team != team)
                {
                    if(getLinfDistance(x, y, i, j) < min_dist)
                    {
                        min_dist = getLinfDistance(x, y, i, j);
                        min_pos = new Vector3Int(i, j, 0);
                    }
                }
        
        if (min_dist <= attackRange) return new Vector3Int(x, y, 0);

        return min_pos;
    }

// apeleaza attackNearestEnemy pentru fiecare monstru
public void attackTick()
    {
        for(int i=0; i<Width; i++)
            for(int j=0; j<Height; j++)
                if(Creatures[i,j] != null)
                    attackNearestEnemy(i, j);
    }

// verifica intr-un box 3x3 daca exista un inamic si ataca (todo: adapteaza dupa range-ul creature-ului)
    public void attackNearestEnemy(int x, int y)
    {
        if(Creatures[x, y] == null) return;

        for(int i = -1; i < 2; i++)
            for(int j = -1; j < 2; j++)
            {
                int x2 = x + i;
                int y2 = y + j;
                if(IsInsideBounds(x2, y2) && (x2 != x || y2 != y) && Creatures[x2, y2] != null)
                {
                    CreatureData attacking_creature = Creatures[x, y].GetComponent<CreatureData>();
                    CreatureData damaged_creature = Creatures[x2, y2].GetComponent<CreatureData>();

                    if(attacking_creature.team == damaged_creature.team) continue;

                    damaged_creature.TakeDamage(attacking_creature.Attack);
                    return;
                }
            }
    }

    public void removeDeadCreatures()
    {
        for(int i=0; i<Width; i++) for(int j=0; j<Height; j++)
            if(Creatures[i, j] != null && Creatures[i, j].GetComponent<CreatureData>().Health < 0)
                RemoveCreature(i, j);
    }

    public int getLinfDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
    }

    public int getDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x2 - x1) + Mathf.Abs(y2 - y1);
    }

    public bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public void StartCombat()
    {
        if (Creatures == null)
        {
            Debug.LogError("Cannot start combat: grid not initialized.");
            return;
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject creature = Creatures[x, y];

                if (creature == null)
                    continue;

                CreatureMovement movement =
                    creature.GetComponent<CreatureMovement>();

                if (movement != null)
                    movement.SetCombatActive(true);
            }
        }

        UpdateCreaturesPath();
    }

    public void StopCombat()
    {
        if (Creatures == null)
            return;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject creature = Creatures[x, y];

                if (creature == null)
                    continue;

                CreatureMovement movement =
                    creature.GetComponent<CreatureMovement>();

                if (movement != null)
                    movement.SetCombatActive(false);
            }
        }
    }
}
