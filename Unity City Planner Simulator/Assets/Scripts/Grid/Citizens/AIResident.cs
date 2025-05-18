using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using System;
using Unity.VisualScripting;
using JetBrains.Annotations;
using Unity.Hierarchy;

public class AIResident : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int movementRadius = 5;
    [SerializeField] private int minIdleWait = 3;
    [SerializeField] private int maxIdleWait = 10;

    [Header("References")]
    [SerializeField] protected Tilemap groundTilemap;
    [SerializeField] protected Tilemap roadTilemap;
    [SerializeField] protected Animator animator;
    [SerializeField] private LayerMask carLayer;
    [SerializeField] private List<Sprite> citizenSprites;

    [Header("Car Detection")]
    [SerializeField] private float carDetectionRadius = 3f;


    private const int ACCELERATION_SPEED = 7;
    private const int DEFAULT_SPEED = 2;

    private float healthInterval = 10f;
    private float healthTimer = 0f;
    public float HealthTimer { get => healthTimer; set => healthTimer = value; }

    protected MovementController movementController;
    protected PathFinder pathFinder;
    protected SpriteRenderer spriteRenderer;
    protected CircleCollider2D carDetectionCollider;

    public int residentID;
    public bool isCommitingCrime = false;
    public bool isHavingHeartAttack = false;

    public SpriteRenderer SpriteRenderer { get { return spriteRenderer; } }
    public MovementController MovementController { get; private set; }
    protected virtual void Start()
    {
        ValidateStartingPosition();
    }
    protected virtual void Awake()
    {
        groundTilemap = ResidentsManager.Instance.groundTilemap;
        roadTilemap = ResidentsManager.Instance.roadTilemap;
        InitializeComponents();
        Debug.Log("Everything is initialized");
    }

    protected virtual void Update()
    {
        if (isHavingHeartAttack)
        {
            healthTimer += Time.deltaTime;
            if (healthTimer >= healthInterval)
            {
                ResetHealthAttack(false);
            }
        }
        if (!isHavingHeartAttack)
        {
            movementController.UpdateMovement();
        }
    }

    public void Initialize(Tilemap groundTilemap, Tilemap roadTilemap, Animator animator)
    {
        Debug.Log($"Initialize called on {gameObject.name}");

        if (groundTilemap == null)
            Debug.LogError($"{gameObject.name}: groundTilemap is null during Initialize");
        if (roadTilemap == null)
            Debug.LogError($"{gameObject.name}: roadTilemap is null during Initialize");
        if (animator == null)
            Debug.LogError($"{gameObject.name}: animator is null during Initialize");

        this.groundTilemap = groundTilemap;
        this.roadTilemap = roadTilemap;
        this.animator = animator;

        if (this.groundTilemap != null && this.roadTilemap != null && this.animator != null)
        {
            InitializeComponents();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Not initializing components due to missing dependencies");
        }
    }

    public void InitializeComponents()
    {
        Debug.Log($"InitializeComponents called on {gameObject.name}");

        if (groundTilemap == null)
        {
            Debug.LogError($"{gameObject.name}: groundTilemap is still null in InitializeComponents");
            groundTilemap = ResidentsManager.Instance.groundTilemap;
        }

        if (roadTilemap == null)
        {
            Debug.LogError($"{gameObject.name}: roadTilemap is still null in InitializeComponents");
            roadTilemap = ResidentsManager.Instance.roadTilemap;
        }

        if (animator == null)
        {
            Debug.Log($"{gameObject.name}: animator is still null in InitializeComponents");
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"{gameObject.name}: Failed to find Animator component");
                return;
            }
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: Missing SpriteRenderer. Adding one.");
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        SetupCarDetection();

        pathFinder = new PathFinder(groundTilemap, roadTilemap);

        movementController = new MovementController(this, pathFinder, animator, spriteRenderer, moveSpeed, movementRadius, minIdleWait, maxIdleWait);

        if (movementController == null)
        {
            Debug.LogError($"{gameObject.name}: Failed to create movement controller");
        }
        else
        {
            Debug.Log($"{gameObject.name}: Movement controller created successfully");
            movementController.OnDestinationReached += ResetCriminal;
        }
    }


    protected void ValidateStartingPosition()
    {
        Vector3Int currentCell = groundTilemap.WorldToCell(transform.position);

        if (!pathFinder.IsValidPosition(currentCell))
        {
            Vector3Int validPosition = pathFinder.FindNearestValidPosition(currentCell);
            transform.position = groundTilemap.GetCellCenterWorld(validPosition);
        }
    }


    protected void SetupCarDetection()
    {
        carDetectionCollider = GetComponent<CircleCollider2D>();
        if (carDetectionCollider == null)
        {
            carDetectionCollider = gameObject.AddComponent<CircleCollider2D>();
            carDetectionCollider.radius = carDetectionRadius;
            carDetectionCollider.isTrigger = true;
        }
    }

    protected void SetDestination(Vector3 target)
    {
        movementController.SetDestination(target);
    }

    public void InitiateCrime(Building building)
    {
        if (isCommitingCrime) return;

        isCommitingCrime = true;

        if (movementController == null)
        {
            Debug.LogWarning("Movement controller is not initialized");
            InitializeComponents();
        }

        spriteRenderer.color = Color.red;
        SetDestination(building.transform.position - new Vector3(0.1f, 0, 0));
            
    }
    internal void InitiateHealthAttack()
    {
        if (isHavingHeartAttack) return;

        isHavingHeartAttack = true;

        spriteRenderer.color = Color.blue;

    }

    public void ResetCriminal(bool isCaught)
    {
        isCommitingCrime = false;
        if (isCommitingCrime && !isCaught)
        {
            FindAnyObjectByType<RobbingSceneIntro>().StartRobbingSequence();
        }


        spriteRenderer.color = Color.white;
        movementController.ChooseNewRandomDestination();
    }

    public void ResetHealthAttack(bool isCured)
    {

        if (isHavingHeartAttack && !isCured)
        {
            PopulationRateManager.Instance.DecreaseRate(1);
            ResidentsManager.Instance.RemoveResident(this);
            Destroy(gameObject);
            FindAnyObjectByType<MedicalAttack>().StartMedicalAttack();
        }

        isHavingHeartAttack = false;
        healthTimer = 0f;

        spriteRenderer.color = Color.white;
        movementController.ChooseNewRandomDestination();
    }
}
