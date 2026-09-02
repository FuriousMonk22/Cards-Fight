using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static TerrainTilemap TerrainTM;
    public static CreaturesGrid creaturesGrid;

    public static int GetLinfDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
    }

    public static int GetManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static Vector3Int GetNearestEnemyInRange(Vector3Int cell, int range, int team)
    {
        float best_distance = 999f;
        Vector3Int best_cell = cell;

        for(int i = -range; i <= range; i++)
            for(int j = -range; j <= range; j++) {
                int x = cell.x + i;
                int y = cell.y + j;


                if(!creaturesGrid.IsInsideBounds(x, y)) continue;
                if(creaturesGrid.Creatures[x, y] == null) continue;
                if(creaturesGrid.Creatures[x, y].GetComponent<Creature>().team == team) continue;
                
                float current_distance = GetManhattanDistance(cell, new Vector3Int(x, y, 0));
                if(current_distance < best_distance)
                {
                    best_distance = current_distance; 
                    best_cell = new Vector3Int(x, y, 0);
                }
            }
        
        return best_cell;
    }

    public static bool CanCreatureTraverse(Vector3Int cell, Creature creature)
    {
        if (TerrainTM == null || creature == null || creature.creatureData == null)
            return false;

        TerrainTileData tile = TerrainTM.GetTileData(cell);
        if (tile == null)
            return false;

        if (creature.creatureData.canFly)
            return true;

        if (creature.creatureData.canWalk && tile.isWalkable)
            return true;

        if (creature.creatureData.canSwim && tile.isSwimmable)
            return true;

        return false;
    }

    public static Vector3Int GetNextStep(Vector3Int start, Vector3Int target, Creature creature)
    {
        List<Vector3Int> path = FindPath(start, target, creature);
        if (path == null || path.Count == 0)
            return start;

        return path[0];
    }

    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int target, Creature creature)
    {
        if (creature == null || creature.creatureData == null)
            return null;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(start);
        visited.Add(start);

        Vector3Int closestReachableCell = start;
        int closestDistance = GetManhattanDistance(start, target);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int distanceToTarget = GetManhattanDistance(current, target);

            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                closestReachableCell = current;
            }

            if (current == target)
                return ReconstructPath(cameFrom, start, target);

            foreach (Vector3Int neighbour in GetNeighbours(current))
            {
                if (visited.Contains(neighbour))
                    continue;

                if (creaturesGrid != null && !creaturesGrid.IsInsideBounds(neighbour.x, neighbour.y))
                    continue;

                if (creaturesGrid != null &&
                    creaturesGrid.Creatures != null &&
                    creaturesGrid.Creatures[neighbour.x, neighbour.y] != null &&
                    creaturesGrid.Creatures[neighbour.x, neighbour.y] != creature.gameObject)
                {
                    continue;
                }

                if (!CanCreatureTraverse(neighbour, creature))
                    continue;

                visited.Add(neighbour);
                cameFrom[neighbour] = current;
                queue.Enqueue(neighbour);
            }
        }

        if (closestReachableCell != start)
            return ReconstructPath(cameFrom, start, closestReachableCell);

        return null;
    }

    public static int GetTerrainPathDistance(CreaturesGrid grid, Vector3Int start, Vector3Int target, Creature creature)
    {
        if (grid == null || creature == null || creature.creatureData == null)
            return -1;

        if (start == target)
            return 0;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        queue.Enqueue(start);
        distances[start] = 0;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int currentDistance = distances[current];

            foreach (Vector3Int direction in new[]
                     {
                         Vector3Int.right,
                         Vector3Int.left,
                         Vector3Int.up,
                         Vector3Int.down
                     })
            {
                Vector3Int next = current + direction;

                if (!grid.IsInsideBounds(next.x, next.y))
                    continue;

                if (distances.ContainsKey(next))
                    continue;

                if (grid.Creatures != null &&
                    grid.Creatures[next.x, next.y] != null &&
                    grid.Creatures[next.x, next.y] != creature.gameObject)
                {
                    continue;
                }

                if (!CanCreatureTraverse(next, creature))
                    continue;

                distances[next] = currentDistance + 1;

                if (next == target)
                    return distances[next];

                queue.Enqueue(next);
            }
        }

        return -1;
    }

    public static Vector3Int GetBestEnemyApproachCell(CreaturesGrid grid, int x, int y, HashSet<Vector3Int> claimedApproachCells)
    {
        if (grid == null || grid.Creatures == null)
            return new Vector3Int(x, y, 0);

        GameObject creatureGO = grid.Creatures[x, y];
        if (creatureGO == null)
            return new Vector3Int(x, y, 0);

        Creature creature = creatureGO.GetComponent<Creature>();
        if (creature == null || creature.creatureData == null)
            return new Vector3Int(x, y, 0);

        Vector3Int start = new Vector3Int(x, y, 0);
        int team = creature.team;

        for (int enemyX = 0; enemyX < grid.Width; enemyX++)
        {
            for (int enemyY = 0; enemyY < grid.Height; enemyY++)
            {
                GameObject enemy = grid.Creatures[enemyX, enemyY];
                if (enemy == null)
                    continue;

                Creature enemyData = enemy.GetComponent<Creature>();
                if (enemyData == null || enemyData.team == team)
                    continue;

                int attackRange = Mathf.Max(0, creature.creatureData.AttackRange);
                if (GetLinfDistance(x, y, enemyX, enemyY) <= attackRange)
                    return start;
            }
        }

        Vector3Int bestTarget = start;
        int bestTargetDistance = int.MaxValue;
        bool foundTarget = false;

        for (int enemyX = 0; enemyX < grid.Width; enemyX++)
        {
            for (int enemyY = 0; enemyY < grid.Height; enemyY++)
            {
                GameObject enemy = grid.Creatures[enemyX, enemyY];
                if (enemy == null)
                    continue;

                Creature enemyData = enemy.GetComponent<Creature>();
                if (enemyData == null || enemyData.team == team)
                    continue;

                Vector3Int enemyPosition = new Vector3Int(enemyX, enemyY, 0);
                Vector3Int bestFreeCell = start;
                int bestFreeDistance = int.MaxValue;
                bool foundFreeCell = false;
                Vector3Int bestBlockedCell = start;
                int bestBlockedDistance = int.MaxValue;
                bool foundBlockedCell = false;

                int attackRange = Mathf.Max(0, creature.creatureData.AttackRange);

                for (int dx = -attackRange; dx <= attackRange; dx++)
                {
                    for (int dy = -attackRange; dy <= attackRange; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        Vector3Int candidate = enemyPosition + new Vector3Int(dx, dy, 0);
                        if (!grid.IsInsideBounds(candidate.x, candidate.y))
                            continue;

                        if (!CanCreatureTraverse(candidate, creature))
                            continue;

                        int pathDistance = GetTerrainPathDistance(grid, start, candidate, creature);
                        if (pathDistance < 0)
                            continue;

                        GameObject occupant = grid.Creatures[candidate.x, candidate.y];
                        bool occupied = occupant != null && occupant != creature.gameObject;
                        bool claimed = claimedApproachCells.Contains(candidate);
                        bool blocked = occupied || claimed;

                        if (!blocked)
                        {
                            if (pathDistance < bestFreeDistance)
                            {
                                bestFreeDistance = pathDistance;
                                bestFreeCell = candidate;
                                foundFreeCell = true;
                            }
                        }
                        else if (pathDistance < bestBlockedDistance)
                        {
                            bestBlockedDistance = pathDistance;
                            bestBlockedCell = candidate;
                            foundBlockedCell = true;
                        }
                    }
                }

                Vector3Int enemyTarget;
                int enemyDistance;

                if (foundFreeCell)
                {
                    enemyTarget = bestFreeCell;
                    enemyDistance = bestFreeDistance;
                }
                else if (foundBlockedCell)
                {
                    enemyTarget = bestBlockedCell;
                    enemyDistance = bestBlockedDistance;
                }
                else
                {
                    continue;
                }

                if (enemyDistance < bestTargetDistance)
                {
                    bestTargetDistance = enemyDistance;
                    bestTarget = enemyTarget;
                    foundTarget = true;
                }
            }
        }

        if (!foundTarget)
            return start;

        GameObject bestOccupant = grid.Creatures[bestTarget.x, bestTarget.y];

        bool bestIsFree =
            (bestOccupant == null || bestOccupant == creature.gameObject)
            && !claimedApproachCells.Contains(bestTarget);

        if (bestIsFree)
            claimedApproachCells.Add(bestTarget);

        return bestTarget;
    }

    private static List<Vector3Int> GetNeighbours(Vector3Int cell)
    {
        return new List<Vector3Int>
        {
            cell + Vector3Int.right,
            cell + Vector3Int.left,
            cell + Vector3Int.up,
            cell + Vector3Int.down
        };
    }

    private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int start, Vector3Int target)
    {
        List<Vector3Int> finalPath = new List<Vector3Int>();
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
