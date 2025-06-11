using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EconomyManager : MonoBehaviour, IEconomyManager, ISaveable
{
    private const int STARTING_MONEY = 1000000;

    [SerializeField] private CityRateManager _cityRateManager;
    [SerializeField] private TMPro.TMP_Text _currentMoneyText;
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private float _timeIntervalInSeconds = 30f;

    private int _currentMoney;
    private BuildingRegistry _buildingRegistry;
    private EconomyCalculator _economyCalculator;
    private TimeManager _timeManager;
    private float _intervalTimer = 0f;

    public event Action OnMoneyChanged;

    public int CurrentMoney => _currentMoney;
    public IReadOnlyList<Building> RegisteredBuildings => _buildingRegistry.GetAllBuildings();
    public IReadOnlyList<GameObject> RegisteredResidents => _buildingRegistry.GetAllResidents();
    public static EconomyManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _buildingRegistry = new BuildingRegistry();
        _economyCalculator = new EconomyCalculator();
        _timeManager = FindAnyObjectByType<TimeManager>();

        _currentMoney = STARTING_MONEY;
    }

    private void Start()
    {
        _economyCalculator.Initialize(_buildingRegistry, Instance, FindAnyObjectByType<PopulationRateManager>());

        UpdateUI();
    }

    private void Update()
    {

        _intervalTimer += Time.deltaTime;
        if (_intervalTimer >= _timeIntervalInSeconds)
        {
            _intervalTimer = 0f;  
            HandleIntervalElapsed();
        }
    }

    private void HandleIntervalElapsed()
    {
        _economyCalculator.CalculateMonthlyEconomics();
        _cityRateManager.UpdateAllRates();
    }

    public bool CanAfford(int cost)
    {
        return _currentMoney >= cost;
    }

    public void AddMoney(int amount)
    {
        _currentMoney = Mathf.Max(0, _currentMoney + amount);
        OnMoneyChanged?.Invoke();
    }

    public bool SubtractMoney(int amount)
    {
        if (_currentMoney < amount)
        {
            return false;
        }
        _currentMoney = Mathf.Max(0, _currentMoney - amount);
        OnMoneyChanged?.Invoke();
        return true;
    }

    private void UpdateUI()
    {
        if (_currentMoneyText != null)
        {
            _currentMoneyText.text = _currentMoney.ToString();
        }
    }

    private void OnEnable()
    {
        OnMoneyChanged += UpdateUI;
    }

    private void OnDisable()
    {
        OnMoneyChanged -= UpdateUI;
        SaveManager.Instance.Unregister(this);
    }

    public void RegisterBuilding(Building building)
    {
        _buildingRegistry.RegisterBuilding(building);
    }

    public void UnregisterBuilding(Building building)
    {
        _buildingRegistry.UnregisterBuilding(building);
    }

    public void RegisterResident(GameObject resident)
    {
        _buildingRegistry.RegisterResident(resident);
    }

    public void UnregisterResident(GameObject resident)
    {
        _buildingRegistry.UnregisterResident(resident);
    }

    public void Save(SaveData data)
    {
        data.money = _currentMoney;
    }

    public void Load(SaveData data)
    {
        _currentMoney = data.money;
        OnMoneyChanged?.Invoke();
        UpdateUI();
    }
}
