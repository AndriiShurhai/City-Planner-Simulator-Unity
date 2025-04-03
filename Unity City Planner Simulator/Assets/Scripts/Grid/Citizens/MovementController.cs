using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementController
{
    private readonly AIResident resident;
    private readonly PathFinder pathFinder;
    private readonly Animator animator;
    private readonly SpriteRenderer spriteRenderer;

    private Vector3? currentDestination;
    private List<Vector3> path = new List<Vector3>();
    private int currentPathIndex = 0;

    private bool isMoving;
    private bool isWaiting;
    private float currentWaitTime;
    private float waitTimeTarget;

    private readonly int movementRadius;
    private readonly int minIdleWait;
    private readonly int maxIdleWait;
    private float moveSpeed;
    private float chaseSpeed = 3.5f;
    private float normalSpeed;
    private bool isChasing;

    private Vector3 lastDirection = Vector3.right;

    public event Action<bool> OnDestinationReached;
    public List<Vector3> Path { get; private set; }
    public int CurrentPathIndex { get; private set; }
    public MovementController(
        AIResident resident,
        PathFinder pathFinder,
        Animator animator,
        SpriteRenderer spriteRenderer,
        float moveSpeed,
        int movementRadius,
        int minIdleWait,
        int maxIdleWait
    )
    {
        this.resident = resident;
        this.pathFinder = pathFinder;
        this.animator = animator;
        this.spriteRenderer = spriteRenderer;
        this.moveSpeed = moveSpeed;
        this.movementRadius = movementRadius;
        this.minIdleWait = minIdleWait;
        this.maxIdleWait = maxIdleWait;
    }

    public void UpdateMovement()
    {
        Debug.Log($"Path count before updateMovement is: {path.Count}");
        if (isWaiting)
        {
            HandleWaiting();
            return;
        }

        if (isMoving && path.Count > 0)
        {
            MoveAlongPath();
        }
        else if (!currentDestination.HasValue ||
                 Vector3.Distance(resident.transform.position, currentDestination.Value) < 0.01f) //
        {
            Debug.Log("Choosing new destination");
            ChooseNewRandomDestination();
        }
        Debug.Log($"Path count after updateMovement is: {path.Count}");
    }

    public void ChooseNewRandomDestination()
    {
        Vector3? newDestination = pathFinder.FindRandomDestination(
            resident.transform.position,
            movementRadius
        );

        if (newDestination.HasValue)
        {
            currentDestination = newDestination.Value;
            SetDestination(newDestination.Value);
        }
        else
        {
            Debug.Log("Lol");
        }
    }

    public void SetDestination(Vector3 target)
    {
        if (pathFinder == null)
        {
            Debug.Log("Path finder is null daaaaamn");
            return;
        }
        if (resident == null)
        {
            Debug.Log("Resident is null daaaamn");
            return;
        }

        Vector3Int startCell = pathFinder.WorldToCell(resident.transform.position);
        Vector3Int targetCell = pathFinder.WorldToCell(target);

        path = pathFinder.FindPath(startCell, targetCell);
        if (path == null)
        {
            path = new List<Vector3>();
            return;
        }
        currentDestination = target;

        currentPathIndex = 0;


        isMoving = true;
        isWaiting = false;

        Debug.Log($"Path for destination is found, path count: {path.Count}");
    }

    public void SetDestinationWithoutBacktracking(Vector3 target)
    {
        Vector3 currentPosition = resident.transform.position;

        Vector3Int startCell = pathFinder.WorldToCell(currentPosition);
        Vector3Int targetCell = pathFinder.WorldToCell(target);

        List<Vector3> newPath = pathFinder.FindPath(startCell, targetCell);
        if (newPath == null || newPath.Count == 0)
        {
            return; 
        }

        int closestPointIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < newPath.Count; i++)
        {
            float distance = Vector3.Distance(currentPosition, newPath[i]);

            float distanceToTarget = Vector3.Distance(newPath[i], target);
            float currentDistanceToTarget = Vector3.Distance(currentPosition, target);

            if (distance < closestDistance && distanceToTarget <= currentDistanceToTarget)
            {
                closestDistance = distance;
                closestPointIndex = i;
            }
        }

        path = newPath;
        currentPathIndex = closestPointIndex;
        currentDestination = target;
        isMoving = true;
        isWaiting = false;

        Debug.Log($"Recalculated chase path, starting from index {currentPathIndex} of {path.Count} points");
    }

    private void UpdateSpriteDirection(Vector3 target)
    {
        Vector3 direction = target - resident.transform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = lastDirection;
        }
        else
        {
            lastDirection = direction.normalized;
        }

        animator.SetBool("IsMovingUp", false);
        animator.SetBool("IsMovingDown", false);
        animator.SetBool("IsMovingLeft", false);
        animator.SetBool("IsMovingRight", false);
        

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                animator.SetBool("IsMovingRight", true);
            else
                animator.SetBool("IsMovingLeft", true);
        }
        else
        {
            if (direction.y > 0)
                animator.SetBool("IsMovingUp", true);
            else
                animator.SetBool("IsMovingDown", true);
        }
    }

    private void MoveAlongPath()
    {
        if (currentPathIndex >= path.Count)
        {
            HandlePathCompletion();

            return;
        }

        Vector3 targetPosition = path[currentPathIndex];
        if (!pathFinder.IsValidPoint(targetPosition))
        {
            HandleInvalidPosition();
            return;
        }

        UpdateSpriteDirection(targetPosition);
        MoveTowardsTarget(targetPosition);
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        resident.transform.position = Vector3.MoveTowards(
            resident.transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(resident.transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
        }
    }

    private void HandleInvalidPosition()
    {
        Debug.Log("Handling invalid position");

        if (!resident.isCommitingCrime)
        {
            currentDestination = null;
        }

        ResetPath();
        isMoving = false;
        StartWaiting(1f);
        isMoving = false;

        Vector3Int validCell = pathFinder.FindNearestValidPosition(
            pathFinder.WorldToCell(resident.transform.position)
        );
        Vector3 validPosition = pathFinder.GetCellCenterWorld(validCell);

        UpdateSpriteDirection(validPosition);

        resident.transform.position = Vector3.MoveTowards(
            resident.transform.position,
            validPosition,
            moveSpeed * Time.deltaTime
        );

        if (resident.isCommitingCrime)
        {
            SetDestination(currentDestination.Value);
        }
    }

    private void HandlePathCompletion()
    {

        OnDestinationReached?.Invoke(false);
        isMoving = false;
        ResetPath();
        StartWaiting();
    }

    private void StartWaiting(float customWaitTarget = 0f)
    {
        isWaiting = true;
        currentWaitTime = 0;
        waitTimeTarget = customWaitTarget > 0 ?
                         customWaitTarget :
                         UnityEngine.Random.Range(minIdleWait, maxIdleWait);
    }

    private void ResetPath()
    {
        currentPathIndex = 0;
        path.Clear();
    }

    private void HandleWaiting()
    {
        currentWaitTime += Time.deltaTime;
        if (currentWaitTime >= waitTimeTarget)
        {
            isWaiting = false;
            isMoving = true;
            currentDestination = null;
        }
    }

    internal void SetChaseSpeed(bool chasing)
    {
        if (chasing && !isChasing)
        {
            moveSpeed = chaseSpeed;
            isChasing = true;
        }

        else if (!chasing && isChasing)
        {
            moveSpeed = 2f;
            isChasing = false;
        }
    }
}
