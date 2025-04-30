using System;
using TMPro;
using UnityEngine;

public class CrimeRateManager : MonoBehaviour, IRate
{
    [SerializeField] private TMP_Text currentCrimeRateTXT;

    private float _crimeRate;
    private float _baseCrimeRate = 20f; // natural crime tendency
    private float _unemploymentImpact = 0.5f;
    private float _educationReduction = 0.2f;
    private float _policeEffectMultiplier = 1f;
    private float _policeEffect;
    private float _alphaCrime = 0.1f;
    private float _targetCrime;


    public event Action OnCrimeRateChange;
    public float CrimeRate { get { return _crimeRate; } }
    public static CrimeRateManager Instance { get; private set; }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        _crimeRate = 70;
        OnCrimeRateChange += UpdateUI;
    }
    public void IncreaseRate(float rate)
    {
        _crimeRate = Mathf.Clamp(_crimeRate + rate, 0, 100);
        OnCrimeRateChange?.Invoke();
    }

    public void DecreaseRate(float rate)
    {
        _crimeRate = Mathf.Clamp(_crimeRate - rate, 0, 100);
        OnCrimeRateChange?.Invoke();
    }

    public void CalculateRate()
    {
        _policeEffect = 0;
        foreach (var building in EconomyManager.Instance.registeredBuildings)
        {
            if (building.BuildingData.buildingType == BuildingType.Police)
            {
                _policeEffect++;
            }
        }
        _policeEffect *= 10;
        _targetCrime = Mathf.Clamp(
            _baseCrimeRate + _unemploymentImpact * UnemploymentRateManager.Instance.UnemploymentRate 
            - _educationReduction * EducationRateManager.Instance.EducationRate 
            - _policeEffectMultiplier * _policeEffect,
            0,
            100);
        // base crime * unemployment impact * unemployment - education's reduction * education - police effect * policeStations * 10

        _crimeRate = _crimeRate + _alphaCrime * (_targetCrime - _crimeRate);
        _crimeRate = Mathf.Clamp(_crimeRate, 0, 100);

        UpdateUI();
    }

    private void UpdateUI()
    {
        currentCrimeRateTXT.text = _crimeRate.ToString();
    }
}
