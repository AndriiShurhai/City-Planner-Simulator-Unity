using TMPro.Examples;
using UnityEngine;

public abstract class ResidentialBuildingBase : Building, IPopulationProvider
{
    protected int currentResidents;
    protected int maxResidents;

    protected override void OnInitialize()
    {
        currentResidents = GetInitialResidentCount();
        maxResidents = GetMaxResidentCount();
    }


    protected abstract int GetInitialResidentCount();  
    protected abstract int GetMaxResidentCount();

    public override int CalculateIncome()
    {
        int taxIncome = buildingData.IncomePerCycle * currentResidents;
        int netIncome = taxIncome - buildingData.MaintenanceCost;
        return netIncome;
    }

    protected override void AfterPlacement()
    {
        if (State != BuildingState.Active) return;
        SpawnInitialResidents();
    }

    protected virtual void SpawnInitialResidents()
    {
        if (OccupiedPositions != null && OccupiedPositions.Count > 0)
        {
            ResidentsManager.Instance.SpawnResidents(currentResidents, (Vector3Int)OccupiedPositions[0]);
        }
    }

    protected override void OnUpgraded()
    {
        if (currentResidents < maxResidents)
        {
            currentResidents++;
            ResidentsManager.Instance.SpawnResidents(1, (Vector3Int)OccupiedPositions[0]);
        }
    }

    public int GetCurrentPopulation() => currentResidents;
    public int GetMaxPopulation() => maxResidents;  

    public void AddPopulation(int amount)
    {
        int newPopulation = Mathf.Min(currentResidents + amount, amount);
        int actualIncrease = newPopulation - currentResidents;

        if (actualIncrease > 0)
        {
            currentResidents = newPopulation;
            if (OccupiedPositions.Count > 0)
            {
                ResidentsManager.Instance.SpawnResidents(actualIncrease, (Vector3Int)OccupiedPositions[0]); 
            }
        }
    }
}
