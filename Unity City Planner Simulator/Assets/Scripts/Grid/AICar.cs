using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CarPathfinding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private List<Sprite> carSprites = new List<Sprite>();

    [SerializeField] int carSpriteIndex = 0;

    private Vector3Int[] directions = {
        new Vector3Int(1, 0, 0),   
        new Vector3Int(-1, 0, 0),  
        new Vector3Int(0, 1, 0),   
        new Vector3Int(0, -1, 0)   
    };

    private List<Vector3> path = new List<Vector3>();
    private int currentPathIndex = 0;
    private bool isMoving = false;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        this.SetDestination(new Vector3(346, -245, 0));
    }

    void Update()
    {
        if (isMoving && path.Count > 0)
        {
            MoveAlongPath();
        }
    }

    public void SetDestination(Vector3 target)
    {
        Vector3Int startCell = roadTilemap.WorldToCell(transform.position);
        Vector3Int targetCell = roadTilemap.WorldToCell(target);

        path = FindPath(startCell, targetCell);
        currentPathIndex = 0;
        isMoving = true;
    }

    private List<Vector3> FindPath(Vector3Int start, Vector3Int target)
    {
        PriorityQueue<Vector3Int> frontier = new PriorityQueue<Vector3Int>();
        frontier.Enqueue(start, 0);

        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var costSoFar = new Dictionary<Vector3Int, float>();

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();

            if (current == target)
                break;

            foreach (Vector3Int direction in directions)
            {
                Vector3Int next = current + direction;

                if (!IsValidRoadTile(next))
                    continue;

                float newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + HeuristicCost(next, target);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        return ReconstructPath(cameFrom, start, target);
    }

    private bool IsValidRoadTile(Vector3Int position)
    {
        return roadTilemap.HasTile(position);
    }

    private float HeuristicCost(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int start, Vector3Int target)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3Int current = target;

        while (current != start)
        {
            path.Add(roadTilemap.GetCellCenterWorld(current));
            current = cameFrom[current];
        }

        path.Add(roadTilemap.GetCellCenterWorld(start));
        path.Reverse();
        return path;
    }

    private void MoveAlongPath()
    {
        if (currentPathIndex >= path.Count)
        {
            isMoving = false;
            return;
        }

        Vector3 targetPosition = path[currentPathIndex];
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                spriteRenderer.sprite = carSprites[0];  // right
            }
            else
            {
                spriteRenderer.sprite = carSprites[2];  // left
            }
        }
        else
        {
            if (direction.y > 0)
            {
                spriteRenderer.sprite = carSprites[3]; // up
            }
            else
            {
                spriteRenderer.sprite = carSprites[1]; // down
            }
        }


        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
        }
    }
}
