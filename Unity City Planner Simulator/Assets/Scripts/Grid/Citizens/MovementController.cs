using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class MovementController : MonoBehaviour
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
            HandleWaititng();
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
            SetDestination(newDestination.Value );
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
        animator.SetBool("isMoving", true);
        UpdateSpriteDirection(target);
    }

    private void UpdateSpriteDirection(Vector3 target)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (target.x < resident.transform.position.x);
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
        isMoving = false;
        animator.SetBool("isMoving", false);
        currentDestination = null;

        ResetPath();
        StartWaiting(1f);

        Vector3Int validPosition = pathFinder.FindNearestValidPosition(
            pathFinder.WorldToCell(resident.transform.position)
        );

        resident.transform.position = Vector3.MoveTowards(
            resident.transform.position,
            pathFinder.GetCellCenterWorld(validPosition),
            moveSpeed * Time.deltaTime
        );
    }

    private void HandlePathCompletion()
    {
        isMoving = false;
        animator.SetBool("isMoving", false);

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

    private void HandleWaititng()
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
