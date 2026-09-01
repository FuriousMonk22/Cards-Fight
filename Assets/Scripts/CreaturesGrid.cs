using System;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;

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
    private Timer ConsoleTimer;

    private void Awake()
    {
        ConsoleTimer = GameObject.FindGameObjectWithTag("GameConsole").GetComponent<Timer>();
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
            Tick();
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

    public void Tick()
    {
        tick_timer -= Time.deltaTime;
        if (tick_timer < 0)
        {
            attackTick();
            removeDeadCreatures();
            tick_timer = tick_length;

            if (getCreatureCount(0) == 0 || getCreatureCount(1) == 0) ConsoleTimer.SkipTimer();
        }
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

        if (Creatures[x1, y1] != null)
        {
            Creatures[x1, y1].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x1, y1, 0));
            Creatures[x1, y1].GetComponent<CreatureMovement>().currentCell = new Vector3Int(x1, y1, 0);
            Creatures[x1, y1].GetComponent<CreatureMovement>().RestartPath();
        }

        if (Creatures[x2, y2] != null)
        {
            Creatures[x2, y2].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x2, y2, 0));
            Creatures[x2, y2].GetComponent<CreatureMovement>().currentCell = new Vector3Int(x2, y2, 0);
            Creatures[x2, y2].GetComponent<CreatureMovement>().RestartPath();
        }

        Debug.Log($"Swapped ({x1}, {y1}) with ({x2}, {y2})");
        UpdateCreaturesPath();
    }

    public bool Spawn(string creatureName, int x, int y)
    {
        if (GamePhaseManager.Instance != null &&
            !GamePhaseManager.Instance.CanPlaceCreatures)
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

        creature.team = y < Height / 2.0 ? 1 : 0;

        if (creature.TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
        {
            renderer.color = creature.team == 1
                ? new Color(1f, 0.3f, 0.4f)
                : new Color(0.3f, 0.3f, 1f);
        }

        Creatures[x, y] = go;

        CreatureMovement movement = go.AddComponent<CreatureMovement>();
        movement.Initialize(this, spawnCell);

        UpdateCreaturesPath();

        return true;
    }

    public void RemoveCreature(int x, int y)
    {
        if (!IsInsideBounds(x, y))
            return;

        GameObject creature = Creatures[x, y];

        if (creature == null)
            return;

        // ============================
        // Ștergem orice rezervare
        // aparținând acestei creaturi.
        // ============================

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (Reservations[i, j] == creature)
                {
                    Reservations[i, j] = null;
                }
            }
        }

        // IMPORTANT:
        // Scoatem imediat creatura din grid.
        //
        // Nu așteptăm Destroy(), deoarece Destroy
        // se execută efectiv la sfârșitul frame-ului.
        Creatures[x, y] = null;

        Destroy(creature);
    }

    public void UpdateCreaturesPath()
    {
        // Celule de atac deja alese în recalcularea curentă.
        // Astfel două creaturi nu aleg simultan aceeași
        // poziție liberă de lângă inamic.
        HashSet<Vector3Int> claimedApproachCells =
            new HashSet<Vector3Int>();

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

                if (!movement.IsMovingBetweenCells)
                {
                    movement.currentCell =
                        new Vector3Int(i, j, 0);
                }

                movement.enemyCell =
                    GetBestEnemyApproachCell(
                        i,
                        j,
                        claimedApproachCells
                    );

                movement.RestartPath();
            }
        }
    }

    private int GetTerrainPathDistance(
    Vector3Int start,
    Vector3Int target,
    Creature creature)
    {
        CreatureData creatureData = creature.creatureData;
        if (start == target)
            return 0;

        Queue<Vector3Int> queue =
            new Queue<Vector3Int>();

        Dictionary<Vector3Int, int> distances =
            new Dictionary<Vector3Int, int>();

        queue.Enqueue(start);
        distances[start] = 0;

        Vector3Int[] directions =
        {
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };

        while (queue.Count > 0)
        {
            Vector3Int current =
                queue.Dequeue();

            int currentDistance =
                distances[current];

            foreach (Vector3Int direction in directions)
            {
                Vector3Int next =
                    current + direction;

                if (!IsInsideBounds(next.x, next.y))
                    continue;

                if (distances.ContainsKey(next))
                    continue;

                // IMPORTANT:
                // aici verificăm DOAR terenul.
                //
                // Creaturile nu sunt pereți permanenți.
                if (!TerrainTM.CanCreatureTraverse(
                        next,
                        creatureData))
                {
                    continue;
                }

                distances[next] =
                    currentDistance + 1;

                if (next == target)
                {
                    return distances[next];
                }

                queue.Enqueue(next);
            }
        }

        // Nu există absolut niciun drum
        // până la această poziție.
        return -1;
    }


    public Vector3Int GetBestEnemyApproachCell(
    int x,
    int y,
    HashSet<Vector3Int> claimedApproachCells)
    {
        GameObject creatureGO = Creatures[x, y];

        if (creatureGO == null)
            return new Vector3Int(x, y, 0);

        Creature creature = creatureGO.GetComponent<Creature>();

        int team = creature.team;

        Vector3Int start =
            new Vector3Int(x, y, 0);

        // Dacă suntem deja lângă un adversar,
        // nu mai trebuie să ne mișcăm.
        for (int enemyX = 0; enemyX < Width; enemyX++)
        {
            for (int enemyY = 0; enemyY < Height; enemyY++)
            {
                GameObject enemy =
                    Creatures[enemyX, enemyY];

                if (enemy == null)
                    continue;

                Creature enemyData =
                    enemy.GetComponent<Creature>();

                if (enemyData.team == team)
                    continue;

                int attackRange =
                    Mathf.Max(0, creature.creatureData.AttackRange);

                if (getLinfDistance(
                        x,
                        y,
                        enemyX,
                        enemyY) <= attackRange)
                {
                    return start;
                }
            }
        }

        Vector3Int bestTarget = start;
        int bestTargetDistance = int.MaxValue;

        bool foundTarget = false;

        // ===============================
        // Verificăm fiecare adversar.
        // ===============================

        for (int enemyX = 0; enemyX < Width; enemyX++)
        {
            for (int enemyY = 0; enemyY < Height; enemyY++)
            {
                GameObject enemy = Creatures[enemyX, enemyY];

                if (enemy == null) continue;

                Creature enemyData = enemy.GetComponent<Creature>();

                if (enemyData.team == team) continue;

                Vector3Int enemyPosition =
                    new Vector3Int(
                        enemyX,
                        enemyY,
                        0
                    );

                // Pentru acest adversar căutăm:
                //
                // 1. cea mai apropiată poziție LIBERĂ
                //    la care putem ajunge
                //
                // 2. dacă toate sunt ocupate,
                //    cea mai apropiată poziție ocupată
                //    ca fallback

                Vector3Int bestFreeCell =
                    start;

                int bestFreeDistance =
                    int.MaxValue;

                bool foundFreeCell =
                    false;

                Vector3Int bestBlockedCell =
                    start;

                int bestBlockedDistance =
                    int.MaxValue;

                bool foundBlockedCell =
                    false;

                // Orice celulă aflată în AttackRange față de adversar
                // este o poziție validă de atac.
                // Folosim distanța L-infinity, la fel ca la atac:
                // AttackRange = 1 -> box 3x3
                // AttackRange = 2 -> box 5x5
                // etc.
                int attackRange =
                    Mathf.Max(0, creature.creatureData.AttackRange);

                for (int dx = -attackRange;
                     dx <= attackRange;
                     dx++)
                {
                    for (int dy = -attackRange;
                         dy <= attackRange;
                         dy++)
                    {
                        // Celula adversarului este ocupată chiar de el,
                        // deci nu poate fi destinație de movement.
                        if (dx == 0 && dy == 0)
                            continue;

                        Vector3Int candidate =
                            enemyPosition +
                            new Vector3Int(
                                dx,
                                dy,
                                0
                            );

                        if (!IsInsideBounds(
                                candidate.x,
                                candidate.y))
                        {
                            continue;
                        }

                        // Poziția trebuie să fie teren
                        // pe care creatura noastră îl poate folosi.
                        if (!TerrainTM.CanCreatureTraverse(
                                candidate,
                                creature.creatureData))
                        {
                            continue;
                        }

                        // =========================
                        // DISTANȚA REALĂ
                        // =========================

                        int pathDistance =
                            GetTerrainPathDistance(
                                start,
                                candidate,
                                creature
                            );

                        // Nu există drum până acolo.
                        if (pathDistance < 0)
                            continue;

                        GameObject occupant =
                            Creatures[
                                candidate.x,
                                candidate.y
                            ];

                        GameObject reservation =
                            Reservations[
                                candidate.x,
                                candidate.y
                            ];

                        bool occupied =
                            occupant != null &&
                            occupant != creature;

                        bool reserved =
                            reservation != null &&
                            reservation != creature;

                        bool claimed =
                            claimedApproachCells.Contains(
                                candidate
                            );

                        bool blocked =
                            occupied ||
                            reserved ||
                            claimed;

                        // =========================
                        // CELULĂ LIBERĂ
                        // =========================

                        if (!blocked)
                        {
                            if (pathDistance <
                                bestFreeDistance)
                            {
                                bestFreeDistance =
                                    pathDistance;

                                bestFreeCell =
                                    candidate;

                                foundFreeCell =
                                    true;
                            }
                        }

                        // =========================
                        // FALLBACK OCUPAT
                        // =========================

                        else
                        {
                            if (pathDistance <
                                bestBlockedDistance)
                            {
                                bestBlockedDistance =
                                    pathDistance;

                                bestBlockedCell =
                                    candidate;

                                foundBlockedCell =
                                    true;
                            }
                        }
                    }
                }

                // =========================
                // Alegerea pentru adversarul acesta
                // =========================

                Vector3Int enemyTarget;
                int enemyDistance;

                if (foundFreeCell)
                {
                    enemyTarget =
                        bestFreeCell;

                    enemyDistance =
                        bestFreeDistance;
                }
                else if (foundBlockedCell)
                {
                    enemyTarget =
                        bestBlockedCell;

                    enemyDistance =
                        bestBlockedDistance;
                }
                else
                {
                    // Adversarul acesta nu poate
                    // fi atins deloc de creatură.
                    continue;
                }

                // =========================
                // Comparăm cu ceilalți adversari
                // DUPĂ DISTANȚA DE PATH.
                // =========================

                if (enemyDistance <
                    bestTargetDistance)
                {
                    bestTargetDistance =
                        enemyDistance;

                    bestTarget =
                        enemyTarget;

                    foundTarget =
                        true;
                }
            }
        }

        // Nu există niciun adversar accesibil.
        if (!foundTarget)
            return start;

        // Dacă poziția aleasă este liberă,
        // o revendicăm pentru această recalculare.
        GameObject bestOccupant =
            Creatures[
                bestTarget.x,
                bestTarget.y
            ];

        GameObject bestReservation =
            Reservations[
                bestTarget.x,
                bestTarget.y
            ];

        bool bestIsFree =
            (bestOccupant == null ||
             bestOccupant == creature)
            &&
            (bestReservation == null ||
             bestReservation == creature)
            &&
            !claimedApproachCells.Contains(
                bestTarget
            );

        if (bestIsFree)
        {
            claimedApproachCells.Add(
                bestTarget
            );
        }

        return bestTarget;
    }

    // apeleaza attackNearestEnemy pentru fiecare monstru
    public void attackTick()
    {
        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Height; j++)
                if (Creatures[i, j] != null)
                    attackNearestEnemy(i, j);
    }

    // Caută cel mai apropiat inamic aflat în AttackRange și îl atacă.
    // Range-ul folosește distanța L-infinity:
    // 1 = box 3x3, 2 = box 5x5, 3 = box 7x7 etc.
    public void attackNearestEnemy(int x, int y)
    {
        GameObject attacker = Creatures[x, y];

        if (attacker == null)
            return;

        Creature attackingCreature =
            attacker.GetComponent<Creature>();

        if (attackingCreature == null)
            return;

        int attackRange =
            Mathf.Max(0, attackingCreature.creatureData.AttackRange);

        GameObject nearestEnemy = null;
        int nearestDistance = int.MaxValue;

        for (int dx = -attackRange;
             dx <= attackRange;
             dx++)
        {
            for (int dy = -attackRange;
                 dy <= attackRange;
                 dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int enemyX = x + dx;
                int enemyY = y + dy;

                if (!IsInsideBounds(enemyX, enemyY))
                    continue;

                GameObject possibleEnemy =
                    Creatures[enemyX, enemyY];

                if (possibleEnemy == null)
                    continue;

                Creature enemyData =
                    possibleEnemy.GetComponent<Creature>();

                if (enemyData == null ||
                    enemyData.team == attackingCreature.team)
                {
                    continue;
                }

                int distance =
                    getLinfDistance(
                        x,
                        y,
                        enemyX,
                        enemyY
                    );

                if (distance > attackRange)
                    continue;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = possibleEnemy;
                }
            }
        }

        if (nearestEnemy == null) return;

        Creature damagedCreature = nearestEnemy.GetComponent<Creature>();

        damagedCreature.TakeDamage( attackingCreature.creatureData.Attack );
    }

    public void removeDeadCreatures()
    {
        bool creatureDied = false;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                GameObject creature = Creatures[i, j];

                if (creature == null) continue;

                Creature data = creature.GetComponent<Creature>();

                if (data == null) continue;

                if (data.getHealth() <= 0)
                {
                    RemoveCreature(i, j);

                    creatureDied = true;
                }
            }
        }

        // IMPORTANT:
        // După ce au fost eliminați TOȚI morții,
        // recalculăm target-urile tuturor supraviețuitorilor.
        if (creatureDied)
        {
            UpdateCreaturesPath();
        }
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