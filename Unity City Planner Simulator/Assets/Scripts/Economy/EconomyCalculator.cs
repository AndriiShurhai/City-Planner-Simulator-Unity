using Unity.VisualScripting;
using UnityEngine;

public class EconomyCalculator : MonoBehaviour
{
    [SerializeField] private float _monthlyIncomeTax = 750f;
    [SerializeField] private float _monthlyPropertyTax = 5f;

    private IBuildingRegistry _buildingRegistry;
    private IEconomyManager _economyManager;
    private PopulationRateManager _populationRateManager;

    private float _monthlyRevenue;
    private float _monthlyExpenses;

    public void Initialize(IBuildingRegistry buildingRegistry, IEconomyManager economyManager, PopulationRateManager populationRate)
    {
        _buildingRegistry = buildingRegistry;
        _economyManager = economyManager;
        _populationRateManager = populationRate;
    }

    public void CalculateMonthlyEconomics()
    {
        CalculateRevenue();
        CalculateExpenses();
        UpdateBudget();
    }

    private void CalculateRevenue()
    {
        float workingPopulation = _populationRateManager.CurrentPopulationRate;
        _monthlyRevenue = workingPopulation * _monthlyIncomeTax;
        _monthlyRevenue += _buildingRegistry.GetBuildingCount() * _monthlyPropertyTax;
    }

    private void CalculateExpenses()
    {
        _monthlyExpenses = 0;
        foreach(var building in _buildingRegistry.GetAllBuildings())
        {
            building.ProcessTick();
            _monthlyExpenses += building.BuildingData.MaintenanceCost;
        }
    }

    private void UpdateBudget()
    {
        int monthlyBalance = Mathf.RoundToInt(_monthlyRevenue - _monthlyExpenses);
        _economyManager.AddMoney(monthlyBalance);
    }

    public float GetMonthlyRevenue() => _monthlyRevenue;
    public float GetMonthlyExpenses() => _monthlyExpenses;
}
