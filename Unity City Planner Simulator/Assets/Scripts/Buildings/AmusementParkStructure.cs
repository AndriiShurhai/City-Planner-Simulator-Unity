using UnityEngine;

public class AmusementParkStructure : EntertainmentBuildingBase
{
    private float visitorCapacity;
    private int currentVisitors;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        happinessBonus = 2f;
        visitorCapacity = 50f * Size.x * Size.y;
        currentVisitors = 0;
    }

    protected override void OnProcessTick()
    {
        base.OnProcessTick();

        SimulateVisitors();
    }

    private void SimulateVisitors()
    {
        float cityHappiness = HappinessRateManager.Instance.HapppinessRate;
        float attractionFactor = happinessBonus * cityHappiness * 0.1f;

        int newVisitors = Mathf.RoundToInt(attractionFactor);
        currentVisitors = Mathf.Min(currentVisitors + newVisitors, Mathf.RoundToInt(visitorCapacity));

        if (EconomyManager.Instance != null && currentVisitors > 0)
        {
            int visitorIncome = Mathf.RoundToInt(currentVisitors * 0.1f);
            EconomyManager.Instance.AddMoney(visitorIncome);
        }
    }

    protected override void OnUpgraded()
    {
        base.OnUpgraded();

        visitorCapacity *= 1.5f;
    }
}
