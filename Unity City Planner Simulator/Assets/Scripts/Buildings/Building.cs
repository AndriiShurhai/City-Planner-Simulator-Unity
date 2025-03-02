using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] public BuildingData buildingData;

    private Vector2Int gridPosition;
    private Vector2Int size;
    private readonly List<BuildingEffectBase> buildingEffects = new List<BuildingEffectBase>();
    protected List<Vector2Int> occupiedPositions;
    private bool isInitialized;

    public event Action OnUpgrade;
    public BuildingData BuildingData => buildingData;
    public Vector2Int GridPosition => gridPosition;
    public Vector2Int Size => size;
    public IReadOnlyList<BuildingEffectBase> BuildingEffects => buildingEffects;
    public IReadOnlyList<Vector2Int> OccupiedPositions => occupiedPositions;

    public virtual void Initialize(BuildingData buildingData, Vector2Int size)
    {
        if (isInitialized) return;

        this.buildingData = buildingData;
        this.size = size;
        this.isInitialized = true;

        name = $"{buildingData.buildingName} ({GetInstanceID()})";

        AddBuildingEffect(new PlacingBuildingOneTimeEffect());
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
        UpdateOccupiedPositions();
    }

    private void UpdateOccupiedPositions()
    {
        occupiedPositions = new List<Vector2Int>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                occupiedPositions.Add(new Vector2Int(gridPosition.x + x, gridPosition.y + y));
            }
        }
    }

    public virtual void OnPlaced()
    {
        if (!isInitialized) return;

        foreach (var effect in buildingEffects)
        {
            effect.OnPlaced(this);
        }
        EconomyManager.Instance.RegisterBuilding(this);
    }

    public virtual void ProcessTick()
    {
        if (!isInitialized) return;

        int netIncome = CalculateIncome();

        if (netIncome != 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(netIncome);
        }

        foreach (var effect in buildingEffects)
        {
            effect.ProcessTick(this);
        }
    }

    public virtual int CalculateIncome()
    {
        if (buildingData == null) return 0;

        return buildingData.incomePerCycle - buildingData.maintenanceCost;
    }

    public bool AddBuildingEffect(BuildingEffectBase buildingEffect)
    {
        if (buildingEffect == null) return false;

        if (!buildingEffects.Contains(buildingEffect))
        {
            buildingEffects.Add(buildingEffect);
            return true;
        }

        return false;
    }

    public bool RemoveBuildingEffect<T>() where T : BuildingEffectBase
    {
        for (int i = 0;  i < buildingEffects.Count; i++)
        {
            if (buildingEffects[i] is T)
            {
                buildingEffects.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public virtual void DestroyBuilding()
    {
        Debug.Log("Trying to destroy building");
        if (GridCity.Instance != null)
        {
            GridCity.Instance.RemoveBuilding(this, occupiedPositions);
        }

        Destroy(gameObject);
    }
}
