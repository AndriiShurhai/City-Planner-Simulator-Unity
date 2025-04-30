using UnityEngine;
using System;
using System.Collections.Generic;

public interface IBuildingEffect
{
    void OnPlaced(Building building);
    void ProcessTick(Building building);
    void Remove(Building building);
}

public abstract class BuildingEffectBase : IBuildingEffect
{
    public virtual void OnPlaced(Building building) { }
    public virtual void ProcessTick(Building building) { }
    public virtual void Remove(Building building) { }
}

public static class BuildingEffectFactory
{
    public static List<IBuildingEffect> CreateEffectsForBuilding(Building building)
    {
        var effects = new List<IBuildingEffect>();

        effects.Add(new CoreBuildingEffect());

        switch (building.BuildingData.buildingType)
        {
            case BuildingType.Residential:
                effects.Add(new ResidentialEffect());
                break;
            case BuildingType.Commercial:
                effects.Add(new CommercialEffect());
                effects.Add(new EmploymentEffect());
                break;
            case BuildingType.Amusement:
                effects.Add(new AmusementEffect());
                effects.Add(new HappinessEffect());
                break;
            case BuildingType.Medical:
                effects.Add(new MedicalEffect());
                effects.Add(new HealthEffect());
                break;
            case BuildingType.Flat:
                effects.Add(new FlatEffect());
                break;
        }

        effects.Add(new UpgradeEffect(building));

        return effects;
    }
}

public class CoreBuildingEffect : BuildingEffectBase
{
    private bool _hasProcessedInitialization = false;

    public override void OnPlaced(Building building)
    {
        if (!_hasProcessedInitialization)
        {
            _hasProcessedInitialization = true;
            Debug.Log($"Core initialization for {building.BuildingData.buildingType}");
        }
    }
}

public class ResidentialEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        Debug.Log("Applied boost for residential house");
    }
}

public class FlatEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        Debug.Log("Applied boost for flat");
    }
}

public class CommercialEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        Debug.Log("Applied commercial building effect");
    }
}

public class AmusementEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        Debug.Log("Applied amusement building effect");
    }
}

public class MedicalEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        Debug.Log("Applied medical building effect");
    }
}

public class EmploymentEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        UnemploymentRateManager.Instance.DecreaseRate(1f);
    }

    public override void Remove(Building building)
    {
        UnemploymentRateManager.Instance.IncreaseRate(1f);
    }
}

public class HappinessEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        HappinessRateManager.Instance.IncreaseRate(1f);
    }

    public override void Remove(Building building)
    {
        HappinessRateManager.Instance.DecreaseRate(1f);
    }

    public override void ProcessTick(Building building)
    {
        // HappinessRateManager.Instance.IncreaseRate(0.1f * Time.deltaTime);
    }
}

public class HealthEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        HealthRateManager.Instance.IncreaseRate(1f);
    }

    public override void Remove(Building building)
    {
        HealthRateManager.Instance.DecreaseRate(1f);
    }
}

public class UpgradeEffect : BuildingEffectBase
{
    private Building _building;

    public UpgradeEffect(Building building)
    {
        _building = building;
        _building.OnUpgrade += HandleUpgrade;
    }

    private void HandleUpgrade()
    {
        Debug.Log($"Building upgraded: {_building.BuildingData.buildingType}");

        switch (_building.BuildingData.buildingType)
        {
            case BuildingType.Residential:
                break;
            case BuildingType.Commercial:
                UnemploymentRateManager.Instance.DecreaseRate(0.5f); 
                break;
            case BuildingType.Amusement:
                HappinessRateManager.Instance.IncreaseRate(0.5f);
                break;
            case BuildingType.Medical:
                HealthRateManager.Instance.IncreaseRate(0.5f);
                break;
        }
    }

    public override void Remove(Building building)
    {
        _building.OnUpgrade -= HandleUpgrade;
    }
}