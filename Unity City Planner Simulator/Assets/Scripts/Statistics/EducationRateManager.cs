using System;
using TMPro;
using UnityEngine;

public class EducationRateManager : MonoBehaviour, IRate
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

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
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
        foreach (var building in EconomyManager.Instance.registeredBuildings)
        {
            if (building.BuildingData.buildingType == BuildingType.Education)
            {
                _educationFacilities++;
            }
        }
        _educationFacilities *= 10;

        _targetEducation = Mathf.Clamp(_baseEducation + _educationFacilitiesMultiplier * _educationFacilities, 0, 100);

        _currentEducationRate = _currentEducationRate + _alphaEducation * (_targetEducation - _currentEducationRate);

        _currentEducationRate = Mathf.Clamp(_currentEducationRate, 0, 100);
    }
    private void UpdateUI()
    {
        currentEducationRateTXT.text = _currentEducationRate.ToString();
    }
}
