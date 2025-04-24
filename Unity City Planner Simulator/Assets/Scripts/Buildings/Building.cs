using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Building Configuration")]
    [SerializeField] public BuildingData buildingData;


    public BuildingData BuildingData => buildingData;
    public Vector2Int GridPosition => _gridPosition;
    public Vector2Int Size => _size;
    public IReadOnlyList<IBuildingEffect> BuildingEffects => _buildingEffects;
    public List<Vector2Int> OccupiedPositions => _occupiedPositions;
    public bool IsInitialized => _isInitialized;
    public bool WasPlacedBefore => _wasPlacedBefore;

    private Vector2Int _gridPosition;
    private Vector2Int _size;
    private readonly List<IBuildingEffect> _buildingEffects = new List<IBuildingEffect>();
    private readonly List<Vector2Int> _occupiedPositions = new List<Vector2Int>();
    private Vector2Int _lastGridPosition;
    private bool _isInitialized;
    private bool _wasPlacedBefore;
    private bool _isPlaced;

    public static event Action<Building> OnBuildingPlaced;
    public static event Action<Building> OnBuildingDestroyed;

    public event Action OnUpgrade;
    private void Awake()
    {
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void OnDestroy()
    {
        CleanupEffects();
        OnBuildingDestroyed?.Invoke(this);
    }



    public virtual void Initialize(BuildingData data, Vector2Int size)
    {
        if (_isInitialized || _wasPlacedBefore) return;

        this.buildingData = data;
        _size = size;
        _isInitialized = true;

        name = $"{data.buildingName} ({GetInstanceID()})";

        SetupBuildingEffects();
    }

    private void SetupBuildingEffects()
    {
        var effects = BuildingEffectFactory.CreateEffectsForBuilding(this);
        foreach (var effect in effects)
        {
            AddBuildingEffect(effect);
        }
    }



    public void SetGridPosition(Vector2Int gridPosition)
    {
        _lastGridPosition = _gridPosition;
        _gridPosition = gridPosition;
        UpdateOccupiedPositions();
    }

    private void UpdateOccupiedPositions()
    {
        _occupiedPositions.Clear();

        for (int x = 0; x < _size.x; x++)
        {
            for (int y = 0; y < _size.y; y++)
            {
                _occupiedPositions.Add(new Vector2Int(_gridPosition.x + x, _gridPosition.y + y));
            }
        }
    }



    public virtual void OnPlaced()
    {
        if (!_isInitialized || _wasPlacedBefore) return;

        _isPlaced = true;
        _wasPlacedBefore = true;

        foreach (var effect in _buildingEffects)
        {
            effect.OnPlaced(this);
        }

        if (ZoneManager.Instance != null)
        {
            Debug.Log("Registring Building");
            ZoneManager.Instance.RegisterBuilding(this);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.RegisterBuilding(this);
        }

        OnBuildingPlaced?.Invoke(this);

        Debug.Log($"Building {buildingData.buildingName} placed at positions: {string.Join(", ", _occupiedPositions)}");
    }

    public virtual void ProcessTick()
    {
        if (!_isInitialized || !_isPlaced) return;

        int netIncome = CalculateIncome();

        if (netIncome != 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(netIncome);
        }

        foreach (var effect in _buildingEffects)
        {
            effect.ProcessTick(this);
        }
    }

    public virtual int CalculateIncome()
    {
        if (buildingData == null) return 0;

        return buildingData.incomePerCycle - buildingData.maintenanceCost;
    }

    public virtual void Upgrade()
    {
        if (buildingData.upgradeLevel >= buildingData.maxUpgradeLevel)
        {
            Debug.Log($"Building {buildingData.buildingName} is already at max level");
        }

        if (EconomyManager.Instance != null && !EconomyManager.Instance.SubtractMoney(buildingData.upgradeCost))
        {
            Debug.Log("Not enough money for upgrade");
            return;
        }

        buildingData.upgradeLevel++;

        buildingData.incomePerCycle = Mathf.RoundToInt(buildingData.incomePerCycle * 1.5f);
        buildingData.upgradeCost = Mathf.RoundToInt(buildingData.upgradeCost * 2f);

        Debug.Log($"Upgraded {buildingData.buildingName} to level {buildingData.upgradeLevel}");

        OnUpgrade?.Invoke();
    }


    public bool AddBuildingEffect(IBuildingEffect buildingEffect)
    {
        if (buildingEffect == null) return false;

        if (!_buildingEffects.Contains(buildingEffect))
        {
            _buildingEffects.Add(buildingEffect);

            if (_isPlaced)
            {
                buildingEffect.OnPlaced(this);
            }

            return true;
        }

        return false;
    }

    public bool RemoveBuildingEffect<T>() where T : IBuildingEffect
    {
        for (int i = 0; i < _buildingEffects.Count; i++)
        {
            if (_buildingEffects[i] is T effect)
            {
                effect.Remove(this);
                _buildingEffects.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    private void CleanupEffects()
    {
        foreach (var effect in _buildingEffects)
        {
            effect.Remove(this);
        }
        _buildingEffects.Clear();
    }


    public virtual void DestroyBuilding()
    {
        Debug.Log($"Destroying building: {buildingData.buildingName}");

        if (GridCity.Instance != null)
        {
            GridCity.Instance.RemoveBuilding(this, _occupiedPositions);
        }

        if (EconomyManager.Instance != null &&
            EconomyManager.Instance.registeredBuildings.Contains(this))
        {
            EconomyManager.Instance.registeredBuildings.Remove(this);
        }

        Destroy(gameObject);
    }

    public virtual void MoveBuilding()
    {
        StartMove();
    }

    private void StartMove()
    {
        if (BuildingMover.Instance != null &&
            BuildingMover.Instance.StartMovingBuilding(this))
        {
            Debug.Log($"Started moving {buildingData.buildingName}");

        }
    }
}