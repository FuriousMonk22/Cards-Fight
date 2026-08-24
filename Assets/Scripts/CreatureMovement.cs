using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureMovement : MonoBehaviour
{
    [Header("Grid")]
    private Tilemap groundTilemap;
    private TerrainTilemap terrainTilemap;
    private CreatureData creatureData;
    //[SerializeField] private Tilemap obstacleTilemap;

    [Header("Target")]
    [SerializeField] public Vector3Int enemyCell;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    private List<Vector3Int> path;

    private int currentPathIndex = 0;
    private bool moving = false;

    private CreaturesGrid creaturesGrid;
    public Vector3Int currentCell;

    private bool hasReservedTarget = false;
    public Vector3Int reservedCell;
    private bool pathUpdatePending = false;


    public bool IsMovingBetweenCells => moving && hasReservedTarget;

    private bool combatActive = false;

    public void Initialize(
    CreaturesGrid grid,
    Vector3Int spawnCell)
    {
        creaturesGrid = grid;

        groundTilemap =
            GridManager.Instance.GroundTilemap;

        terrainTilemap =
            groundTilemap.GetComponent<TerrainTilemap>();

        creatureData =
            GetComponent<CreatureData>();

        currentCell = spawnCell;

        transform.position =
            groundTilemap.GetCellCenterWorld(currentCell);

        moving = false;
    }

    void Update()
    {
        if (!combatActive)
            return;

        if (!moving)
            return;

        MoveAlongPath();
    }

    public void SetCombatActive(bool active)
    {
        combatActive = active;

        if (active)
            return;

        // Combatul s-a terminat

        moving = false;
        pathUpdatePending = false;
        path = null;
        currentPathIndex = 0;

        // Dacă eram în drum spre o celulă,
        // anulăm rezervarea.
        if (hasReservedTarget)
        {
            if (creaturesGrid.IsInsideBounds(
                reservedCell.x,
                reservedCell.y))
            {
                if (creaturesGrid.Reservations[
                    reservedCell.x,
                    reservedCell.y] == gameObject)
                {
                    creaturesGrid.Reservations[
                        reservedCell.x,
                        reservedCell.y] = null;
                }
            }

            hasReservedTarget = false;
        }

        // Revenim exact în centrul celulei
        // pe care grid-ul ne consideră încă.
        transform.position =
            groundTilemap.GetCellCenterWorld(currentCell);
    }

    //    public void RestartPath()
    //    {
    //        creaturesGrid.Reservations[reservedCell.x, reservedCell.y] = null;
    //        StartPathfinding();
    //    }

    public void RestartPath()
    {
        if (!combatActive)
            return;

        // If we're currently moving between cells,
        // don't interrupt that movement.
        if (moving && hasReservedTarget)
        {
            pathUpdatePending = true;
            return;
        }

        RestartPathImmediately();
    }

    private void RestartPathImmediately()
    {
        pathUpdatePending = false;

        if (hasReservedTarget)
        {
            if (creaturesGrid.IsInsideBounds(
                reservedCell.x,
                reservedCell.y))
            {
                if (creaturesGrid.Reservations[
                    reservedCell.x,
                    reservedCell.y] == gameObject)
                {
                    creaturesGrid.Reservations[
                        reservedCell.x,
                        reservedCell.y] = null;
                }
            }

            hasReservedTarget = false;
        }

        StartPathfinding();
    }

    public void StartPathfinding()
    {
        if (!combatActive)
            return;

        moving = false;

        Vector3Int startCell = currentCell;

        transform.position =
            groundTilemap.GetCellCenterWorld(startCell);

        path = FindPath(startCell, enemyCell);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Nu există drum către inamic!");
            return;
        }

        currentPathIndex = 0;
        moving = true;
    }
    void MoveAlongPath()
    {
        if (currentPathIndex >= path.Count)
        {
            moving = false;
            Debug.Log("Am ajuns la destinație!");
            return;
        }

        Vector3Int targetCell = path[currentPathIndex];

        // Înainte să ne mișcăm, rezervăm celula
        if (!hasReservedTarget)
        {
            bool reserved =
                creaturesGrid.TryReserveCell(
                    gameObject,
                    targetCell
                );

            // Altă creatură ocupă / a rezervat celula.
            // Așteptăm pe poziția actuală.
            if (!reserved)
            {
                return;
            }

            reservedCell = targetCell;
            hasReservedTarget = true;
        }

        Vector3 targetPosition =
            groundTilemap.GetCellCenterWorld(reservedCell);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
            transform.position,
            targetPosition) < 0.001f)
        {
            transform.position = targetPosition;

            // Acum mutarea este sigură.
            creaturesGrid.CompleteMove(
                gameObject,
                currentCell,
                reservedCell
            );

            currentCell = reservedCell;

            hasReservedTarget = false;

            currentPathIndex++;

            // We have reached a stable grid cell.
            // It is now safe to recalculate the path.
            if (pathUpdatePending)
            {
                RestartPathImmediately();
            }
        }
    }

    // ==============================
    // BFS
    // ==============================
    List<Vector3Int> FindPath(
    Vector3Int start,
    Vector3Int target)
    {
        Queue<Vector3Int> queue =
            new Queue<Vector3Int>();

        HashSet<Vector3Int> visited =
            new HashSet<Vector3Int>();

        Dictionary<Vector3Int, Vector3Int> cameFrom =
            new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(start);
        visited.Add(start);

        // Dacă target-ul este imposibil de atins,
        // păstrăm cea mai apropiată celulă accesibilă
        // de target dintre cele găsite de BFS.
        Vector3Int closestReachableCell = start;

        int closestDistance =
            ManhattanDistance(start, target);

        while (queue.Count > 0)
        {
            Vector3Int current =
                queue.Dequeue();

            int distanceToTarget =
                ManhattanDistance(
                    current,
                    target
                );

            if (distanceToTarget < closestDistance)
            {
                closestDistance =
                    distanceToTarget;

                closestReachableCell =
                    current;
            }

            // BFS garantează că prima dată când
            // ajungem la target avem un drum cu
            // număr minim de pași.
            if (current == target)
            {
                return ReconstructPath(
                    cameFrom,
                    start,
                    target
                );
            }

            List<Vector3Int> neighbours =
                GetNeighbours(current);

            // Păstrăm variația dintre drumurile
            // echivalente ca lungime.
            Shuffle(neighbours);

            foreach (Vector3Int neighbour
                     in neighbours)
            {
                if (visited.Contains(neighbour))
                    continue;

                // Terenul imposibil de traversat
                // funcționează ca un perete.
                if (!IsWalkable(
                        neighbour,
                        target))
                {
                    continue;
                }

                visited.Add(neighbour);

                cameFrom[neighbour] =
                    current;

                queue.Enqueue(neighbour);
            }
        }

        // ==========================
        // TARGET IMPOSIBIL
        // ==========================

        // Dacă target-ul nu poate fi atins,
        // mergem până la cea mai apropiată
        // poziție accesibilă găsită.
        if (closestReachableCell != start)
        {
            return ReconstructPath(
                cameFrom,
                start,
                closestReachableCell
            );
        }

        return null;
    }

    void Shuffle(List<Vector3Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            Vector3Int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // ==============================
    // Vecini Manhattan
    // ==============================

    List<Vector3Int> GetNeighbours(Vector3Int cell)
    {
        return new List<Vector3Int>()
        {
            cell + Vector3Int.right,
            cell + Vector3Int.left,
            cell + Vector3Int.up,
            cell + Vector3Int.down
        };
    }

    // ==============================
    // Walkable
    // ==============================

    bool IsWalkable(
    Vector3Int cell,
    Vector3Int target)
    {
        if (!creaturesGrid.IsInsideBounds(
                cell.x,
                cell.y))
        {
            return false;
        }

        if (!terrainTilemap.CanCreatureTraverse(
                cell,
                creatureData))
        {
            return false;
        }

        return true;
    }

    // ==============================
    // Manhattan distance
    // ==============================

    int ManhattanDistance(
        Vector3Int a,
        Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) +
               Mathf.Abs(a.y - b.y);
    }

    // ==============================
    // Construim drumul final
    // ==============================

    List<Vector3Int> ReconstructPath(
        Dictionary<Vector3Int, Vector3Int> cameFrom,
        Vector3Int start,
        Vector3Int target)
    {
        List<Vector3Int> finalPath =
            new List<Vector3Int>();

        Vector3Int current = target;

        while (current != start)
        {
            finalPath.Add(current);

            current = cameFrom[current];
        }

        finalPath.Reverse();

        return finalPath;
    }
}