using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CarPathfinding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Tilemap roadTilemap;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private List<string> leftLaneTiles = new List<string> { "tilemap_288", "tilemap_313" };
    [SerializeField] private List<string> rightLaneTiles = new List<string> { "tilemap_290", "tilemap_265" };
    [SerializeField] private List<string> dividerLaneTiles = new List<string> { "tilemap_294", "tilemap_271", "tilemap_296", "tilemap_319", "tilemap_295"};

    [SerializeField] public List<Sprite> carSprites = new List<Sprite>();

    [SerializeField] int carSpriteIndex = 0;

    public System.Action OnDestinationReached;

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

    private enum RoadType
    {
        LeftLane,
        RightLane,
        DividerLane
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isMoving && path.Count > 0)
        {
            MoveAlongPath();
        }
    }

    public void SetDestination(Vector3 start, Vector3 target)
    {
        transform.position = start;
        Vector3Int startCell = GetCorrectLane(roadTilemap.WorldToCell(start));
        Vector3Int targetCell = GetCorrectLane(roadTilemap.WorldToCell(target));

        path = FindPath(startCell, targetCell);
        currentPathIndex = 0;
        isMoving = true;
    }

    private Vector3Int GetCorrectLane(Vector3Int cell)
    {
        foreach (Vector3Int direction in directions)
        {
            Vector3Int checkCell = cell + direction;
            if (IsValidLane(checkCell))
            {
                return checkCell;
            }
        }
        return cell;
    }

    private bool IsValidLane(Vector3Int position)
    {
        TileBase tile = roadTilemap.GetTile(position);
        if (tile == null) return false;
        return leftLaneTiles.Contains(tile.name) || rightLaneTiles.Contains(tile.name);
    }

    private RoadType GetRoadType(Vector3Int position)
    {
        TileBase tile = roadTilemap.GetTile(position);

        if (leftLaneTiles.Contains(tile.name)) return RoadType.LeftLane;
        if (rightLaneTiles.Contains(tile.name)) return RoadType.RightLane;
        if (dividerLaneTiles.Contains(tile.name)) return RoadType.DividerLane;

        return RoadType.DividerLane;
    }

    private bool IsCorrectDrivingSide(Vector3Int current, Vector3Int next)
    {
        Vector3Int direction = current - next;
        RoadType roadType = GetRoadType(current);

        if (direction.x > 0 || direction.y > 0)
        {
            return roadType == RoadType.RightLane;
        }
        else if (direction.x < 0 || direction.y < 0)
        {
            return roadType == RoadType.LeftLane;
        }

        return true;
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

                if (GetRoadType(current) != GetRoadType(next) || !IsCorrectDrivingSide(current, next))
                {
                    newCost += 5;
                }

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
        TileBase tile = roadTilemap.GetTile(position);

        if (tile == null)
        {
            return false;
        }

        if (!(leftLaneTiles.Contains(tile.name) && !rightLaneTiles.Contains(tile.name) && !dividerLaneTiles.Contains(tile.name))){
            return tile != null;
        }

        return true;
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
            Vector3 worldPosition = roadTilemap.GetCellCenterWorld(current);
            RoadType roadType = GetRoadType(current);

            Vector3Int direction = cameFrom[current] - current;

            if (direction.x > 0 && roadType == RoadType.RightLane)
            {
                worldPosition += new Vector3(0, 0.4f, 0);
            }

            else if (direction.x < 0 && roadType == RoadType.RightLane)
            {
                worldPosition += new Vector3(0, -0.4f, 0);
            }

            else if(direction.y > 0 && roadType == RoadType.RightLane)
            {
                worldPosition += new Vector3(-0.4f, 0, 0);
            }
            else if (direction.y < 0 && roadType == RoadType.RightLane)
            {
                worldPosition += new Vector3(0.4f, 0, 0);
            }

            path.Add(worldPosition);
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
            OnDestinationReached?.Invoke();
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
