using System;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData buildingData;
    public Vector2Int gridPosition;
    public Vector2Int size;

    public virtual void OnPlaced()
    {
        EconomyManager.Instance.RegisterBuilding(this);
    }

    public virtual void ProcessTick()
    {
        int netIncome = CalculateIncome();
        EconomyManager.Instance.AddMoney(netIncome);
    }

    protected virtual int CalculateIncome()
    {
        return buildingData.incomePerCycle - buildingData.maintenanceCost;
    }
}