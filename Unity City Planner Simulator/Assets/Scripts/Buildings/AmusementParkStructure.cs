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
        AddBuildingEffect(new HappinessRateBoostEffect());
    }

    public override void ProcessTick()
    {
        base.ProcessTick();
        HappinessRateManager.Instance.IncreaseRate(happinessBonus);
    }
}
