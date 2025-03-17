using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinder
{
    private readonly Tilemap groundTilemap;
    private readonly Tilemap roadTilemap;

    private readonly Vector3Int[] directions =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };

    public PathFinder(Tilemap groundTilemap, Tilemap roadTilemap)
    {
        this.groundTilemap = groundTilemap;
        this.roadTilemap = roadTilemap;
    }

    internal Vector3Int FindNearestValidPosition(Vector3Int currentCell)
    {
        for (int radius = 1; radius < 100; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int testPosition = currentCell + new Vector3Int(x, y, 0);
                    if (IsValidCell(testPosition) && !roadTilemap.HasTile(testPosition) &&
                        IsValidPoint(groundTilemap.CellToWorld(testPosition)))
                    {
                        return testPosition; 
                    }   
                }
            }
        }

        Debug.LogWarning("No valid position found for resident");
        return currentCell;
    }

    public List<Vector3> FindPath(Vector3Int startCell, Vector3Int targetCell)
    {
        var frontier = new PriorityQueue<Vector3Int>();
        frontier.Enqueue(startCell, 0);

        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var costSoFar = new Dictionary<Vector3Int, float>();

        cameFrom[startCell] = startCell;
        costSoFar[startCell] = 0;

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();
            if (current == targetCell)
            {
                break;
            }

            foreach(Vector3Int direction in directions)
            {
                Vector3Int next = current + direction;
                if (!IsValidPosition(next)) continue;

                float newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + HeuristicCost(next, targetCell);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        return cameFrom.ContainsKey(targetCell) ?
                ReconstructPath(cameFrom, startCell, targetCell) :
                null;
    }

    private List<Vector3> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int startCell, Vector3Int targetCell)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3Int current = cameFrom[targetCell];

        while (current != startCell)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(groundTilemap.GetCellCenterWorld(startCell));
        path.Reverse();
        return path;
    }

    private float HeuristicCost(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public Vector3? FindRandomDestination(Vector3 position, int movementRadius)
    {
        int maxAttempts = 1000;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector3Int targetCell = GenerateRandomCell(position, movementRadius);
            if (IsValidPosition(targetCell) && !roadTilemap.HasTile(targetCell))
            {
                return groundTilemap.GetCellCenterWorld(targetCell);
            }
            attempts++;
        }

        Debug.Log("Could not find a destination");
        return null;
    }

    private Vector3Int GenerateRandomCell(Vector3 position, int movementRadius)
    {
        int x = UnityEngine.Random.Range(
            Mathf.Max((int)position.x - movementRadius, (int)groundTilemap.localBounds.min.x),
            Mathf.Min((int)position.x + movementRadius, (int)groundTilemap.localBounds.max.x)
        );

        int y = UnityEngine.Random.Range(
            Mathf.Max((int)position.y - movementRadius, (int)groundTilemap.localBounds.min.y),
            Mathf.Min((int)position.y + movementRadius, (int)groundTilemap.localBounds.max.y)
        );

        return new Vector3Int(x, y, 0);
    }

    public Vector3 GetCellCenterWorld(Vector3Int validPosition) => groundTilemap.GetCellCenterWorld(validPosition);

    public bool IsValidPosition(Vector3Int position)
    {
        return IsValidPoint(groundTilemap.CellToWorld(position)) && IsValidCell(position);
    }

    private bool IsValidCell(Vector3Int position)
    {
        TileBase tile = groundTilemap.GetTile(position);

        if (tile == null) return false;

        return !ObstacleRemover.Instance.CheckLargeObstacle(position) &&
               !ObstacleRemover.Instance.CheckMiddleObstacle(position);
    }

    public bool IsValidPoint(Vector3 position)
    {
        Vector2Int cellPosition = (Vector2Int) groundTilemap.WorldToCell(position);
        return !GridCity.Instance.Buildings.ContainsKey(cellPosition);
    }

    public Vector3Int WorldToCell(Vector3 target) => groundTilemap.WorldToCell(target);
}