using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private const int STARTING_MONEY = 10000;


    [SerializeField] TMPro.TMP_Text currentMoneyTXT;
    [SerializeField] AudioManager audioManager;

    private int _currentMoney;
    public List<Building> registeredBuildings;

    public delegate void MoneyChangeHandler(float newMoney);
    public event MoneyChangeHandler OnMoneyChanged;
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
        UpdateUI();
    }

    public bool CanAfford(int cost)
    {
        return _currentMoney >= cost;
    }

    public void AddMoney(int amount)
    {
        if (amount < 0) return;
        _currentMoney += amount;
        OnMoneyChanged?.Invoke(_currentMoney);
        UpdateUI();
    }

    public void SubtractMoney(int amount)
    {
        if (!CanAfford(amount)) return;
        _currentMoney = Mathf.Max(0, _currentMoney - amount);
        OnMoneyChanged?.Invoke(_currentMoney);
        UpdateUI();
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
