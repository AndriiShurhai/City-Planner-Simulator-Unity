using System;
using TMPro;
using UnityEngine;

public class HealthRateManager : MonoBehaviour, IRate
{
    [SerializeField] TMP_Text currentHealthRateTXT;

    private float _healthRate;
    private float _baseHealth = 30f;
    private float _educationContribution = 0.3f;
    private float _healthcareMultiplier = 1f;
    private float _healthcareEffect;
    private float _alphaHealth = 0.1f;
    private float _targetHealth;

    public float HealthRate { get { return _healthRate; } }
    public static HealthRateManager Instance { get; private set; }

    public event Action OnHealthRateChange;


    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        OnHealthRateChange += UpdateUI;
    }
    public void IncreaseRate(float rate)
    {
        _healthRate = Mathf.Clamp(_healthRate + rate, 0, 100);
        OnHealthRateChange?.Invoke();
    }

    public void DecreaseRate(float rate)
    {
        _healthRate = Mathf.Clamp(_healthRate - rate, 0, 100);
        OnHealthRateChange?.Invoke();
    }
    public void CalculateRate()
    {
        _healthcareEffect = 0;
        foreach (var building in EconomyManager.Instance.registeredBuildings)
        {
            if (building.BuildingData.Type == BuildingType.Medical)
            {
                _healthcareEffect++;
            }
        }

        _healthcareEffect *= 10;

        _targetHealth = Mathf.Clamp(_baseHealth + _educationContribution * EducationRateManager.Instance.EducationRate + _healthcareMultiplier * _healthcareEffect, 0, 100);

        _healthRate = _healthRate + _alphaHealth * (_targetHealth - _healthRate);
        _healthRate = Mathf.Clamp(_healthRate, 0, 100);
    }

    private void UpdateUI()
    {
        currentHealthRateTXT.text = _healthRate.ToString();
    }
}
