using System;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] public BuildingData buildingData;
    [SerializeField] public Vector2Int gridPosition;
    public Vector2Int size;

    public virtual void Initialize()
    {

    }
   public virtual void Initialize(BuildingData buildingData, Vector2Int size)
   {
        this.buildingData = buildingData;
        this.size = size;
        DontDestroyOnLoad(gameObject);
   }
    public virtual void OnPlaced()
    {
        EconomyManager.Instance.RegisterBuilding(this);
    }

    public virtual void ProcessTick()
    {
        int netIncome = CalculateIncome();
        EconomyManager.Instance.AddMoney(netIncome);
    }

    public virtual int CalculateIncome()
    {
        return buildingData.incomePerCycle - buildingData.maintenanceCost;
    }
}