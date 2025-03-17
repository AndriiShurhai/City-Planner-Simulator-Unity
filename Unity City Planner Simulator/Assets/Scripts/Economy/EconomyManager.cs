using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private const int STARTING_MONEY = 10000;

    [SerializeField] private CrimeRateManager crimeRateManager;
    [SerializeField] private UnemploymentRateManager unemploymentRateManager;
    [SerializeField] private HappinessRateManager happinessRateManager;
    [SerializeField] private PopulationRateManager populationRateManager;
    [SerializeField] private HealthRateManager healthRateManager;
    [SerializeField] private EducationRateManager educationRateManager;

    [SerializeField] public List<GameObject> rates;

    [SerializeField] TMPro.TMP_Text currentMoneyTXT;
    [SerializeField] AudioManager audioManager;

    [SerializeField] private float monthlyIncomeTax = 750f;
    [SerializeField] private float monthlyPropertyTax = 5f;

    private int _currentMoney;
    private float _monthlyRevenue;
    private float _monthlyExpenses;

    public List<Building> registeredBuildings;
    public List<GameObject> registeredResidents;

    public List<AmusementParkStructure> amusementBuildings;

    public event Action OnMoneyChanged;
    public static EconomyManager Instance { get; private set; }
    public int CurrentMoney { get { return _currentMoney; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _currentMoney = STARTING_MONEY;
        this.OnMoneyChanged += UpdateUI;

        InvokeRepeating(nameof(CalculateMonthlyEconomics), 0f, 5f);

        UpdateUI();
    }

    private void CalculateMonthlyEconomics()
    {
        CalculateRevenue();
        CalculateExpenses();
        UpdateBudgetAndRates();
    }

    private void UpdateBudgetAndRates()
    {
        int monthlyBalance = Mathf.RoundToInt(_monthlyRevenue - _monthlyExpenses);
        AddMoney(monthlyBalance);

        UpdateCityRates();
    }

    private void UpdateCityRates()
    {
        foreach (var rate in rates)
        {
            rate.GetComponent<IRate>().CalculateRate();
            
        }
    }

    private void CalculateExpenses()
    {
        _monthlyExpenses = 0;
        foreach (var building in registeredBuildings)
        {
            building.ProcessTick();
        }
    }

    private void CalculateRevenue()
    {
        float workingPopulation = PopulationRateManager.Instance.CurrentPopulationRate;
        _monthlyRevenue = workingPopulation * monthlyIncomeTax;

        _monthlyRevenue += registeredBuildings.Count * monthlyPropertyTax;
    }

    public bool CanAfford(int cost)
    {
        return _currentMoney >= cost;
    }

    public void AddMoney(int amount)
    {
        _currentMoney += amount;
        OnMoneyChanged?.Invoke();
    }

    public void SubtractMoney(int amount)
    {
        _currentMoney = Mathf.Max(0, _currentMoney - amount);
        OnMoneyChanged?.Invoke();
    }

    public void UpdateUI()
    {
        if (currentMoneyTXT != null)
        {
            currentMoneyTXT.text = _currentMoney.ToString();
        }
    }

    internal void RegisterBuilding(Building building)
    {
        registeredBuildings.Add(building);
        Debug.Log("Placed");
    }
}
