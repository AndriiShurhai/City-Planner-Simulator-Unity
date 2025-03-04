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

    private readonly float moveSpeed;
    private readonly int movementRadius;
    private readonly int minIdleWait;
    private readonly int maxIdleWait;

    // Store the last valid movement direction.
    private Vector3 lastDirection = Vector3.right;

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
                 Vector3.Distance(resident.transform.position, currentDestination.Value) < 0.01f)
        {
            ChooseNewRandomDestination();
        }
    }

    private void ChooseNewRandomDestination()
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
    }

    private void SetDestination(Vector3 target)
    {
        Vector3Int startCell = pathFinder.WorldToCell(resident.transform.position);
        Vector3Int targetCell = pathFinder.WorldToCell(target);

        path = pathFinder.FindPath(startCell, targetCell);
        if (path == null)
        {
            path = new List<Vector3>();
            return;
        }

        currentPathIndex = 0;
        isMoving = true;
    }

    private void UpdateSpriteDirection(Vector3 target)
    {
        // Compute direction from current position to target
        Vector3 direction = target - resident.transform.position;
        // If the movement is too small, use the last nonzero direction to avoid flickering.
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = lastDirection;
        }
        else
        {
            lastDirection = direction.normalized;
        }

        // Reset all animation booleans
        animator.SetBool("IsMovingUp", false);
        animator.SetBool("IsMovingDown", false);
        animator.SetBool("IsMovingLeft", false);
        animator.SetBool("IsMovingRight", false);

        // Determine primary movement axis and set animator accordingly.
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

        // If close enough to the target, proceed to the next waypoint.
        if (Vector3.Distance(resident.transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
        }
    }

    private void HandleInvalidPosition()
    {
        isMoving = false;
        currentDestination = null;

        ResetPath();
        StartWaiting(1f);

        Vector3Int validCell = pathFinder.FindNearestValidPosition(
            pathFinder.WorldToCell(resident.transform.position)
        );
        Vector3 validPosition = pathFinder.GetCellCenterWorld(validCell);

        // Update sprite direction based on the valid cell's center.
        UpdateSpriteDirection(validPosition);

        resident.transform.position = Vector3.MoveTowards(
            resident.transform.position,
            validPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private void HandlePathCompletion()
    {
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
}
