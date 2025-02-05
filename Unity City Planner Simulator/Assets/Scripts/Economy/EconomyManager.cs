using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private int startingMoney;
    [SerializeField] TMPro.TMP_Text currentMoneyTXT;
    [SerializeField] AudioManager audioManager;

    private int currentMoney;
    public List<Building> registeredBuildings;
    public static EconomyManager Instance { get; private set; }
    public TMPro.TMP_Text CurrentMoney { get { return currentMoneyTXT; } }

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
        currentMoney = startingMoney;
        UpdateUI();
    }

    public bool CanAfford(int cost)
    {
        return currentMoney >= cost;
    }

    public void AddMoney(int amount)
    {
        if (amount < 0) return;
        currentMoney += amount;
        UpdateUI();
    }

    public void SubtractMoney(int amount)
    {
        if (!CanAfford(amount)) return;
        currentMoney = Mathf.Max(0, currentMoney - amount);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (currentMoneyTXT != null)
        {
            currentMoneyTXT.text = currentMoney.ToString();
        }
    }

    internal void RegisterBuilding(Building building)
    {
        registeredBuildings.Add(building);
        Debug.Log("Placed");
    }
}
