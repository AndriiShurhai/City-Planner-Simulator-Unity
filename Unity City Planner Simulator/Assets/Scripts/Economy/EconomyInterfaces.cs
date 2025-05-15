using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildingRegistry
{
    void RegisterBuilding(Building building);
    void UnregisterBuilding(Building building);
    int GetBuildingCount();
    IReadOnlyList<Building> GetAllBuildings();
}

public interface IEconomyManager
{
    int CurrentMoney { get; }
    bool CanAfford(int cost);
    bool SubtractMoney(int amount);
    void AddMoney(int amount);
    event Action OnMoneyChanged;
}