using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HappinessRateManager
{
    private float _currentHappinessRate;
    private TMP_Text _currentHappinessRateTXT;
    private Image[] happinessConditionImages;


    public void IncreaseHappinessRatePercentage(float percentage)
    {
        _currentHappinessRate += percentage;
    }

    public void DecreaseHappinessRatePercentage(float percentage)
    {
        _currentHappinessRate -= percentage;
    }

    public void UpdateUI()
    {
        _currentHappinessRateTXT.text = $"{_currentHappinessRate}%";
    }
}
