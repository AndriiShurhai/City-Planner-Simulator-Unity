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
    private bool hasRun = false;
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
                Debug.Log("This is boost for commercial");
                break;
            default:
                Debug.Log("This is boost");
                break;
        }

        if (!hasRun)
        {
            Debug.Log("This is an initialization boost");
            hasRun = true;
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