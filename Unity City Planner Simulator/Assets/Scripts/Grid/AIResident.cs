using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using System;

public class AIResident : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap roadTilemap;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int movementRadius = 5;
    [SerializeField] private Vector3? currentDestination = null;
    [SerializeField] private int minIdleWait = 3;
    [SerializeField] private int maxIdleWait = 10;
    [SerializeField] private bool isWaiting = false;

    private float currentWaitTime = 0f;
    private float waitTimeTarget;
    private float carDetectionRadius = 3f;
    [SerializeField] private LayerMask carLayer;
    private CircleCollider2D carDetectionCollider;
    private int accelerateSpeed = 7;
    private int defaultSpeed = 2;

    [SerializeField] public List<Sprite> citizenSprites = new List<Sprite>();
    [SerializeField] private int citizenSpriteIndex = 0;

    [SerializeField] private Animator animator;

    public System.Action OnDestinationReached;

    private Vector3Int[] directions =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int (1, 1, 0),
        new Vector3Int (-1, -1, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };

    private List<Vector3> path = new List<Vector3>();
    private int currentPathIndex = 0;
    private bool isMoving = false;

    private SpriteRenderer spriteRenderer;

    private enum GroundType
    {
        RoadSafe,
        RoadUnsafe,
        ObstacleBig,
        ObstacleSmall,
        Structure
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        carDetectionCollider = GetComponent<CircleCollider2D>(); 

        if (carDetectionCollider == null)
        {
            carDetectionCollider = gameObject.AddComponent<CircleCollider2D>();
            carDetectionCollider.radius = carDetectionRadius;
            carDetectionCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (IsCarNearby())
        {
            if (isMoving)
            {
                if (roadTilemap.GetTile(roadTilemap.WorldToCell(transform.position)) == null)
                {
                    animator.SetBool("isMoving", false);
                    isMoving = false;
                    moveSpeed = defaultSpeed;
                    return;
                }
                else
                {
                    moveSpeed = accelerateSpeed;
                }
            }
        }
        else
        {
            moveSpeed = defaultSpeed;
            if (!isMoving && path.Count > 0)
            {
                animator.SetBool("isMoving", true);
                isMoving = true;
            }
        }
        if (isWaiting)
        {
            currentWaitTime += Time.deltaTime;
            if (currentWaitTime >= waitTimeTarget)
            {
                isWaiting = false;
                currentWaitTime = 0f;
                currentDestination = null;
            }

            return;
        }

        if (isMoving && path.Count > 0)
        {
            MoveAlongPath();
        }
        else if (!currentDestination.HasValue || Vector3.Distance(transform.position, currentDestination.Value) < 0.1f)
        {
            ChooseNewRandomDestination();
        }
    }

    private bool IsCarNearby()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, carDetectionRadius, carLayer);

        foreach(Collider2D collider in nearbyColliders)
        {
            if (collider.gameObject != gameObject && collider.gameObject.CompareTag("Car"))
            {
                Vector2 directionToCar = (collider.transform.position - transform.position).normalized;

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (directionToCar.x < 0);
                }

                return true;
            }
        }
        return false;
    }
    private void ChooseNewRandomDestination()
    {
        int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            int x = UnityEngine.Random.Range(
                Mathf.Max(-movementRadius, (int)groundTilemap.localBounds.min.x),
                Mathf.Min(movementRadius, (int)groundTilemap.localBounds.max.x)
            );
            int y = UnityEngine.Random.Range(
                Mathf.Max(-movementRadius, (int)groundTilemap.localBounds.min.y),
                Mathf.Min(movementRadius, (int)groundTilemap.localBounds.max.y)
            );

            Vector3Int targetCell = new Vector3Int(x, y, 0);
            if (IsValidCell(targetCell) && !roadTilemap.HasTile(targetCell))
            {
                Vector3 randomPoint = groundTilemap.GetCellCenterWorld(targetCell);
                currentDestination = randomPoint;
                SetDestination(randomPoint);
                return;
            }
            attempts++;
        }

        Debug.LogWarning("Failed to find valid destination after " + maxAttempts + " attempts");
    }

    public void SetDestination(Vector3 target)
    {
        Vector3Int startCell = groundTilemap.WorldToCell(transform.position);
        Vector3Int targetCell = groundTilemap.WorldToCell(target);

        path = FindPath(startCell, targetCell);
        currentPathIndex = 0;
        isMoving = true;
        animator.SetBool("isMoving", true);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (target.x < transform.position.x);
        }
    }

    private bool IsValidCell(Vector3Int position)
    {
        TileBase tile = groundTilemap.GetTile(position);
        if (tile == null) return false;
        if (ObstacleRemover.Instance == null) return tile != null;
        if (ObstacleRemover.Instance.CheckLargeObstacle(position) || ObstacleRemover.Instance.CheckMiddleObstacle(position))
        {
            return false;
        }
        return tile != null;
    }

    private List<Vector3> FindPath(Vector3Int startCell, Vector3Int targetCell)
    {
        PriorityQueue<Vector3Int> frontier = new PriorityQueue<Vector3Int>();
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

            foreach (Vector3Int direction in directions)
            {
                Vector3Int next = current + direction;

                if (!IsValidCell(next)) continue;

                float newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + HeruisticCost(next, targetCell);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        return ReconstructPath(cameFrom, startCell, targetCell);
    }

    private List<Vector3> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int startCell, Vector3Int targetCell)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3Int current = targetCell;

        while (current != startCell)
        {
            Vector3 worldPositon = groundTilemap.GetCellCenterWorld(current);

            path.Add(worldPositon);
            current = cameFrom[current];
        }

        path.Add(groundTilemap.GetCellCenterWorld(startCell));
        path.Reverse();
        return path;
    }

    private float HeruisticCost(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private void MoveAlongPath()
    {
        if (currentPathIndex >= path.Count)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
            OnDestinationReached?.Invoke();
            currentPathIndex = 0;
            path.Clear();

            isWaiting = true;
            currentWaitTime = 0;
            waitTimeTarget = UnityEngine.Random.Range(minIdleWait, maxIdleWait);
            return;
        }

        Vector3 targetPositon = path[currentPathIndex];
        Vector3 direction = (targetPositon - transform.position).normalized;

        transform.position = Vector3.MoveTowards(transform.position, targetPositon, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPositon) < 0.1f)
        {
            currentPathIndex++;
        }
    }
}
