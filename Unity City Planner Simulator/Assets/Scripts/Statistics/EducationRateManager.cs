using System;
using TMPro;
using UnityEngine;

public class EducationRateManager : MonoBehaviour, ICityRate, ISaveable
{
    [SerializeField] private TMP_Text currentEducationRateTXT;

    private float _currentEducationRate;
    private float _baseEducation = 20;
    private float _educationFacilitiesMultiplier = 1;
    private float _educationFacilities;
    private float _targetEducation;
    private float _alphaEducation = 0.05f;

    public event Action OnEducationRateChange; 
    public float EducationRate {  get { return _currentEducationRate; } }
    public static EducationRateManager Instance { get; private set; }

    private void Awake()
    {
        SaveManager.Instance.Register(this);
    }
    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        OnEducationRateChange += UpdateUI;
    }

    public void IncreaseRate(float rate)
    {
        _currentEducationRate = Mathf.Clamp(_currentEducationRate + rate, 0, 100);
        OnEducationRateChange?.Invoke();
    }

    public void DecreaseRate(float rate)
    {
        _currentEducationRate = Mathf.Clamp(_currentEducationRate - rate, 0, 100);
        OnEducationRateChange?.Invoke();
    }

    public void CalculateRate()
    {
        _educationFacilities = 0;
        foreach (var building in EconomyManager.Instance.RegisteredBuildings)
        {
            if (building.BuildingData.Type == BuildingType.Education)
            {
                IEducationProvider educationProvider = building as IEducationProvider;
                if (educationProvider != null)
                {
                    _educationFacilities += educationProvider.GetEducationContribution();
                }
            }
        }

        _targetEducation = Mathf.Clamp(_baseEducation + _educationFacilitiesMultiplier * _educationFacilities, 0, 100);
        _currentEducationRate = _currentEducationRate + _alphaEducation * (_targetEducation - _currentEducationRate);
        _currentEducationRate = Mathf.Clamp(_currentEducationRate, 0, 100);

        UpdateUI();
    }
    private void UpdateUI()
    {
        currentEducationRateTXT.text = _currentEducationRate.ToString();
    }
    public void Save(SaveData data)
    {
        data.educationRate = _currentEducationRate;
    }

    public void Load(SaveData data)
    {
        _currentEducationRate = data.educationRate;
        UpdateUI();
    }
}
