using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

public enum BuildingState { Constructing, Active}
public class Building : MonoBehaviour
{
    [Header("Building Configuration")]
    [SerializeField] protected BuildingData buildingData;


    public BuildingData BuildingData => buildingData;
    public Vector2Int GridPosition => _gridPosition;
    public IReadOnlyList<IBuildingEffect> BuildingEffects => _buildingEffects;
    public List<Vector2Int> OccupiedPositions => _occupiedPositions;
    public bool IsInitialized => _isInitialized;
    public bool WasPlacedBefore => _wasPlacedBefore;

    public BuildingState State { get; private set; } = BuildingState.Constructing;

    protected int _upgradeLevel;
    protected Vector2Int _gridPosition;
    protected readonly List<IBuildingEffect> _buildingEffects = new List<IBuildingEffect>();
    protected readonly List<Vector2Int> _occupiedPositions = new List<Vector2Int>();
    protected Vector2Int _lastGridPosition;
    protected bool _isInitialized;
    protected bool _wasPlacedBefore;
    protected bool _isPlaced;
    protected Coroutine _constructionCoroutine;


    public static event Action<Building> OnBuildingPlaced;
    public static event Action<Building> OnBuildingDestroyed;
    public static event Action<Building> OnBuildingConstruction;
    public event Action<BuildingState> OnStateChanged;
    public event Action OnUpgrade;

    private void Update()
    {
        if (State == BuildingState.Constructing)
        {
            float pulse = Mathf.Sin((Time.time + 1f) * 1f) * 0.5f + 0.5f;
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.gray, Color.cyan, pulse);
        }
    }
    public virtual void Initialize(BuildingData data, Vector2Int size)
    {
        if (_isInitialized || _wasPlacedBefore) return;

        this.buildingData = data;
        _isInitialized = true;

        name = $"{data.BuildingName} ({GetInstanceID()})";

        OnInitialize();

        SetupBuildingEffects();
    }
    protected virtual void OnInitialize() { }
    private void SetupBuildingEffects()
    {
        var effects = BuildingEffectFactory.CreateEffectsForBuilding(this);
        foreach (var effect in effects)
        {
            AddBuildingEffect(effect);
        }
    }
    private void StartConstruction()
    {
        if (buildingData.ConstructionDuration <= 0)
        {
            CompleteConstruction();
            return;
        }
        OnStateChanged?.Invoke(State);
        OnBuildingConstruction?.Invoke(this);
        _constructionCoroutine = StartCoroutine(Construct());
    }
    private IEnumerator Construct()
    {
        yield return new WaitForSeconds(buildingData.ConstructionDuration);
        CompleteConstruction();
    }

    private void CompleteConstruction()
    {
        _constructionCoroutine = null;
        State = BuildingState.Active;
        OnStateChanged?.Invoke(State);
        GetComponent<SpriteRenderer>().color = Color.white;
        OnPlaced();
        Debug.Log("Building placed");
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        _lastGridPosition = _gridPosition;
        _gridPosition = gridPosition;
        UpdateOccupiedPositions();
        StartConstruction();
    }
    private void UpdateOccupiedPositions()
    {
        Debug.Log("Updating occupied positions");
        _occupiedPositions.Clear();

        for (int x = 0; x < buildingData.Size.x; x++)
        {
            for (int y = 0; y < buildingData.Size.y; y++)
            {
                Debug.Log("pos");
                _occupiedPositions.Add(new Vector2Int(_gridPosition.x + x, _gridPosition.y + y));
            }
        }
    }
    public virtual void OnPlaced()
    {
        if (!_isInitialized) return;

        _isPlaced = true;

        foreach (var effect in _buildingEffects)
        {
            effect.OnPlaced(this);
        }

        RegisterWithManagers();

        AfterPlacement();

        OnBuildingPlaced?.Invoke(this);

        Debug.Log($"Building {buildingData.BuildingName} placed at positions: {string.Join(", ", _occupiedPositions)}");
    }

    protected virtual void AfterPlacement() { }

    private void RegisterWithManagers()
    {
        if (ZoneManager.Instance != null)
        {
            Debug.Log("Registring Building");
            ZoneManager.Instance.RegisterBuilding(this);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.RegisterBuilding(this);
        }
    }

    public virtual void ProcessTick()
    {
        if (!_isInitialized || !_isPlaced || State == BuildingState.Constructing) return;

        int netIncome = CalculateIncome();

        if (netIncome != 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(netIncome);
        }

        foreach (var effect in _buildingEffects)
        {
            effect.ProcessTick(this);
        }

        OnProcessTick();
    }

    protected virtual void OnProcessTick() { }

    public virtual int CalculateIncome()
    {
        if (buildingData == null) return 0;

        return buildingData.IncomePerCycle - buildingData.MaintenanceCost;
    }

    public virtual void Upgrade()
    {
        if (_upgradeLevel >= buildingData.MaxUpgradeLevel)
        {
            Debug.Log($"Building {buildingData.BuildingName} is already at max level");
            return;
        }

        if (!TryPayForUpgrade())
        {
            Debug.Log("Not enough money for upgrade");
            return;
        }

        _upgradeLevel++;

        ApplyStandartUpgrades();

        OnUpgraded();

        Debug.Log($"Upgraded {buildingData.BuildingName} to level {_upgradeLevel}");

        OnUpgrade?.Invoke();
    }
    private bool TryPayForUpgrade()
    {
        return (EconomyManager.Instance != null && EconomyManager.Instance.SubtractMoney(buildingData.UpgradeCost));
    }

    private void ApplyStandartUpgrades()
    {
        buildingData.IncomePerCycle = Mathf.RoundToInt(buildingData.IncomePerCycle * 1.5f);
        buildingData.UpgradeCost = Mathf.RoundToInt(buildingData.UpgradeCost * 2f);
    }

    protected virtual void OnUpgraded() { }

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
        Debug.Log($"Destroying building: {buildingData.BuildingName}");

        UnregisterFromManagers();
        CleanupEffects();

        Destroy(gameObject);
    }

    private void UnregisterFromManagers()
    {
        if (GridCity.Instance != null)
        {
            GridCity.Instance.RemoveBuilding(this, _occupiedPositions);
        }

        if (EconomyManager.Instance != null &&
            EconomyManager.Instance.RegisteredBuildings.Contains(this))
        {
            EconomyManager.Instance.UnregisterBuilding(this);
        }
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
            Debug.Log($"Started moving {buildingData.BuildingName}");

        }
    }

    private void OnDestroy()
    {
        CleanupEffects();
        OnBuildingDestroyed?.Invoke(this);
    }
}