using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] public BuildingData buildingData;
    [SerializeField] public Vector2Int gridPosition;
    public Vector2Int size;

    protected List<BuildingEffectBase> buildingEffects = new List<BuildingEffectBase>();

    public Action OnUpgrade;
   public virtual void Initialize(BuildingData buildingData, Vector2Int size)
   {
        this.buildingData = buildingData;
        this.size = size;
        DontDestroyOnLoad(gameObject);

        AddBuildingEffect(new PlacingBuildingOneTimeEffect());
   }
    public virtual void OnPlaced()
    {
        foreach (var effect in buildingEffects)
        {
            effect.OnPlaced(this);
        }
        EconomyManager.Instance.RegisterBuilding(this);
    }

    public virtual void ProcessTick()
    {
        int netIncome = CalculateIncome();
        EconomyManager.Instance.AddMoney(netIncome);

        if (buildingEffects.Count == 0)
        {
            return;
        }

        foreach (var effect in buildingEffects)
        {
            effect.ProcessTick(this);
        }
    }

    public virtual int CalculateIncome()
    {
        return buildingData.incomePerCycle - buildingData.maintenanceCost;
    }

    public void AddBuildingEffect(BuildingEffectBase buildingEffect)
    {
        if (buildingEffect != null && !buildingEffects.Contains(buildingEffect))
        {
            buildingEffects.Add(buildingEffect);
        }
    }
}