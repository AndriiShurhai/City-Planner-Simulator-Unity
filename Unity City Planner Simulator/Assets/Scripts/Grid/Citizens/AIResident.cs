using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using System;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class AIResident : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int movementRadius = 5;
    [SerializeField] private int minIdleWait = 3;
    [SerializeField] private int maxIdleWait = 10;

    [Header("References")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private LayerMask carLayer;
    [SerializeField] private List<Sprite> citizenSprites;
    [SerializeField] private Animator animator;

    [Header("Car Detection")]
    [SerializeField] private float carDetectionRadius = 3f;

    private const int ACCELERATION_SPEED = 7;
    private const int DEFAULT_SPEED = 2;

    private MovementController movementController;
    private PathFinder pathFinder;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D carDetectionCollider;

    public void Initialize(Tilemap groundTilemap, Tilemap roadTilemap, Animator animator)
    {
        this.groundTilemap = groundTilemap;
        this.roadTilemap = roadTilemap;
        this.animator = animator;
    }

    private void Awake()
    {    }

    private void Start()
    {
        InitializeComponents();
        ValidateStartingPosition();
    }

    private void ValidateStartingPosition()
    {
        Vector3Int currentCell = groundTilemap.WorldToCell(transform.position);
        
        if (!pathFinder.IsValidPosition(currentCell))
        {
            Vector3Int validPosition = pathFinder.FindNearestValidPosition(currentCell);
            transform.position = groundTilemap.GetCellCenterWorld(validPosition);
        }
    }

    private void Update()
    {
        movementController.UpdateMovement();
    }
    private void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetupCarDetection();

        pathFinder = new PathFinder(groundTilemap, roadTilemap);
        movementController = new MovementController(this, pathFinder, animator, spriteRenderer, moveSpeed, movementRadius, minIdleWait, maxIdleWait);
    }

    private void SetupCarDetection()
    {
        carDetectionCollider = GetComponent<CircleCollider2D>();
        if (carDetectionCollider == null)
        {
            carDetectionCollider = gameObject.AddComponent<CircleCollider2D>();
            carDetectionCollider.radius = carDetectionRadius;
            carDetectionCollider.isTrigger = true;
        }
    }
}
