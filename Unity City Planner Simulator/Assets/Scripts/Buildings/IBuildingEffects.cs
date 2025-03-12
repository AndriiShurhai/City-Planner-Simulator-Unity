using UnityEngine;

public interface IBuildingEffects
{
    void ProcessTick(Building building);
    void OnPlaced(Building building);
    void Remove(Building building);
}


public abstract class BuildingEffectBase : IBuildingEffects
{
    public virtual void OnPlaced(Building building) { }
    public virtual void ProcessTick(Building building) { }
    public virtual void Remove(Building building) { }
}

public class PlacingBuildingOneTimeEffect : BuildingEffectBase
{
    private bool hasRun;
    public override void OnPlaced(Building building)
    {
        switch (building.buildingData.buildingType)
        {
            case BuildingType.Residential:
                Debug.Log("This is boost for residential house");
                break;

            case BuildingType.Flat:
                Debug.Log("This is boost for flat");
                break;

            case BuildingType.Commercial:
                UnemploymentRateManager.Instance.DecreaseRate(1f);
                break;

            case BuildingType.Amusement:
                HappinessRateManager.Instance.IncreaseRate(1f);
                break;

            case BuildingType.Medical:
                HealthRateManager.Instance.IncreaseRate(1f);
                break;

            default:
                Debug.Log("This is boost");
                break;
        }

        if (!hasRun)
        {
            hasRun = true;
            Debug.Log("This is an initialization boost");
        }
    }
}

public class UpgradeBoostResidentialEffect : BuildingEffectBase
{
    public UpgradeBoostResidentialEffect(Building building)
    {
        building.OnUpgrade += HandleUpgrade;
    }

    private void HandleUpgrade()
    {
        Debug.Log("This is an upgrade");
    }
}

public class HappinessRateBoostEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        foreach (var structure in EconomyManager.Instance.registeredBuildings)
        {
            if (structure.buildingData.buildingType == BuildingType.Amusement)
            { 
            }
        }
    }
}

public class HospitalRateBoostEffect : BuildingEffectBase
{
    public override void OnPlaced(Building building)
    {
        base.OnPlaced(building);
    }

    public override void ProcessTick(Building building)
    {
        base.ProcessTick(building);
    }
}
