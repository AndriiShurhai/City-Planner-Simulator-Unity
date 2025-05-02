using UnityEngine;

public class AmusementParkStructure : Building
{
    [SerializeField] private float happinessBonus = 2f;

    public float HappinessBonus
    {
        get { return happinessBonus; }
        set
        {
            if (value > 0)
            {
                happinessBonus += value;
            }
        }
    }
    public override void Initialize(BuildingData buildingData, Vector2Int size)
    {
        base.Initialize(buildingData, size);
    }

    public override void ProcessTick()
    {
        base.ProcessTick();
        HappinessRateManager.Instance.IncreaseRate(happinessBonus);
    }

    public override void Upgrade()
    {
        base.Upgrade();
        if (buildingData.upgradeLevel >= buildingData.maxUpgradeLevel)
        {
            return;
        }
        happinessBonus += 0.5f;
    }
}
