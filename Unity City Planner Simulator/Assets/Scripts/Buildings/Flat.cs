using System.Collections;
using UnityEngine;

public class Flat : Building
{
    private int startResidents = 5;

    private int currentLevel;
    private int currentResidents;

    public override void Initialize(BuildingData buildingData, Vector2Int size)
    {
        base.Initialize(buildingData, size);
        currentResidents = startResidents;
        currentLevel = 1;
    }

    public override int CalculateIncome()
    {
        int taxIncome = buildingData.incomePerResident * currentResidents;
        int netIncome = taxIncome - buildingData.maintenanceCost;

        return netIncome;
    }

    public override void ProcessTick()
    {
        base.ProcessTick();

        // additional affects
    }
}
