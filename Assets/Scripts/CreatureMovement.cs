using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureMovement : MonoBehaviour
{
    [Header("Grid")]
    private Tilemap groundTilemap;
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

    public void Initialize(
    CreaturesGrid grid,
    Vector3Int spawnCell)
    {
        creaturesGrid = grid;

        groundTilemap =
            GridManager.Instance.GroundTilemap;

        currentCell = spawnCell;

        transform.position =
            groundTilemap.GetCellCenterWorld(currentCell);

        StartPathfinding();
    }

    void Update()
    {
        if (!moving)
            return;

        MoveAlongPath();
    }

//    public void RestartPath()
//    {
//        creaturesGrid.Reservations[reservedCell.x, reservedCell.y] = null;
//        StartPathfinding();
//    }

    public void RestartPath()
    {
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
    // A*
    // ==============================
    List<Vector3Int> FindPath(
        Vector3Int start,
        Vector3Int target)
    {
        List<Vector3Int> openList =
            new List<Vector3Int>();

        HashSet<Vector3Int> closedList =
            new HashSet<Vector3Int>();

        Dictionary<Vector3Int, Vector3Int> cameFrom =
            new Dictionary<Vector3Int, Vector3Int>();

        Dictionary<Vector3Int, int> gCost =
            new Dictionary<Vector3Int, int>();

        openList.Add(start);
        gCost[start] = 0;

        while (openList.Count > 0)
        {
            Vector3Int current =
                GetLowestFCost(openList, gCost, target);

            // Am găsit ținta
            if (current == target)
            {
                return ReconstructPath(
                    cameFrom,
                    start,
                    target
                );
            }

            openList.Remove(current);
            closedList.Add(current);

            // Get neighbours
            List<Vector3Int> neighbours =
                GetNeighbours(current);

            // Randomize neighbour order
            Shuffle(neighbours);

            foreach (Vector3Int neighbour in neighbours)
            {
                if (closedList.Contains(neighbour))
                    continue;

                if (!IsWalkable(neighbour, target))
                    continue;

                int tentativeGCost =
                    gCost[current] + 1;

                if (!gCost.ContainsKey(neighbour) ||
                    tentativeGCost < gCost[neighbour])
                {
                    cameFrom[neighbour] = current;

                    gCost[neighbour] = tentativeGCost;

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
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
        // Permitem întotdeauna celula țintă
        if (cell == target)
            return true;

        // Trebuie să existe ground
        if (!groundTilemap.HasTile(cell))
            return false;

        // Nu trebuie să existe obstacol
        //if (obstacleTilemap != null &&
            //obstacleTilemap.HasTile(cell))
            //return false;

        return true;
    }

    // ==============================
    // F cost
    // ==============================

    Vector3Int GetLowestFCost(
        List<Vector3Int> openList,
        Dictionary<Vector3Int, int> gCost,
        Vector3Int target)
    {
        Vector3Int bestCell = openList[0];

        int bestF =
            gCost[bestCell] +
            ManhattanDistance(bestCell, target);

        foreach (Vector3Int cell in openList)
        {
            int f =
                gCost[cell] +
                ManhattanDistance(cell, target);

            if (f < bestF)
            {
                bestF = f;
                bestCell = cell;
            }
        }

        return bestCell;
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