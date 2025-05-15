using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HappinessRateManager : MonoBehaviour, IRate
{
    private float _currentHappinessRate;
    [SerializeField] private TMP_Text currentHappinessRateTXT;
    [SerializeField] private Image[] happinessConditionImages;
    [SerializeField] private Image[] happinessImagesToChange;

    public event Action OnHappinessRateChange;

    private float _safetyWeight = 3f;
    private float _employmentWeight = 2f;
    private float _healthWeight = 2f;
    private float _educationWeight = 1f;

    public float HapppinessRate { get { return _currentHappinessRate; } }
    public static HappinessRateManager Instance { get; private set; }
    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        _currentHappinessRate = 70f;
        OnHappinessRateChange += UpdateUI;
        OnHappinessRateChange?.Invoke();
    }

    public void CalculateRate()
    {
        float happinessEffect = 0;

        foreach (var building in EconomyManager.Instance.registeredBuildings)
        {
            if (building.BuildingData.Type == BuildingType.Amusement)
            {
                happinessEffect++;
            }
        }
        happinessEffect *= 10;

        _currentHappinessRate = Mathf.Clamp((_safetyWeight * (100 - CrimeRateManager.Instance.CrimeRate)
                                          + _employmentWeight * (100 - UnemploymentRateManager.Instance.UnemploymentRate)
                                          + _healthWeight * HealthRateManager.Instance.HealthRate
                                          + _educationWeight * EducationRateManager.Instance.EducationRate
                                          + 1 * happinessEffect)
                                          / (_safetyWeight + _employmentWeight + _healthWeight + _educationWeight),
                                          0, 100);
        OnHappinessRateChange?.Invoke();
    }
    public void IncreaseRate(float percentage)
    {
        _currentHappinessRate = Mathf.Clamp(_currentHappinessRate + percentage, 0, 100);
        OnHappinessRateChange?.Invoke();
    }

    public void DecreaseRate(float percentage)
    {
        _currentHappinessRate = Mathf.Clamp(_currentHappinessRate - percentage, 0, 100);
        OnHappinessRateChange?.Invoke(); 
    }

    private void UpdateUI()
    {
        currentHappinessRateTXT.text = $"{_currentHappinessRate}%";

        if (0 <= _currentHappinessRate && _currentHappinessRate <= 25)
        {
            UpdateImages(0);
        }
        else if (25 < _currentHappinessRate && _currentHappinessRate <= 50)
        {
            UpdateImages(1);
        }

        else if (50 < _currentHappinessRate && _currentHappinessRate <= 75)
        {
            UpdateImages(2);
        }

        else if(75 < _currentHappinessRate && _currentHappinessRate <= 90)
        {
            UpdateImages(3);
        }
        else if (90 < _currentHappinessRate && _currentHappinessRate <= 100)
        {
            UpdateImages(4);
        }
    }

    private void UpdateImages(int index)
    {
        for (int i = 0; i < happinessImagesToChange.Length; i++)
        {
            happinessImagesToChange[i].sprite = happinessConditionImages[index].sprite;
        }
    }
}
