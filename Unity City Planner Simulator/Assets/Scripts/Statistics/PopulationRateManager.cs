using System;
using TMPro;
using UnityEngine;

public class PopulationRateManager : MonoBehaviour, ICityRate, ISaveable
{
    [SerializeField] private TMP_Text currentPopulationRateTXT;

    private float _currentPopulationRate;

    public event Action OnPopulationRateChange;
    public float CurrentPopulationRate { get { return _currentPopulationRate; } }
    public static PopulationRateManager Instance { get; private set; }

    private int workersCitizens;
    private int policeCitizens;
    private int tourists;

    private void Awake()
    {
        SaveManager.Instance.Register(this);
    }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        OnPopulationRateChange += UpdateUI;
        _currentPopulationRate = EconomyManager.Instance.RegisteredResidents.Count;
    }

    public void IncreaseRate(float rate)
    {
        _currentPopulationRate += rate;
        OnPopulationRateChange?.Invoke();
    }

    public void DecreaseRate(float rate)
    {
        _currentPopulationRate = Mathf.Clamp(_currentPopulationRate - rate, 0, int.MaxValue);
        OnPopulationRateChange?.Invoke();  
    }
    public void CalculateRate()
    {
        _currentPopulationRate = EconomyManager.Instance.RegisteredResidents.Count;
    }
    private void UpdateUI()
    {
        currentPopulationRateTXT.text = _currentPopulationRate.ToString();  
    }

    public void Save(SaveData data)
    {
        data.populationRate = _currentPopulationRate;
    }

    public void Load(SaveData data)
    {
        _currentPopulationRate = data.populationRate;
        UpdateUI();
    }
}
