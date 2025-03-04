using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HappinessRateManager : MonoBehaviour 
{
    private float _currentHappinessRate;
    [SerializeField] private TMP_Text _currentHappinessRateTXT;
    [SerializeField] private Image[] happinessConditionImages;
    [SerializeField] private Image[] happinessImagesToChange;

    public event Action OnHappinessRateChange;

    public void Start()
    {
        OnHappinessRateChange += UpdateUI;
        OnHappinessRateChange?.Invoke();
    }
    public void IncreaseHappinessRatePercentage(float percentage)
    {
        _currentHappinessRate += percentage;
        OnHappinessRateChange?.Invoke();
    }

    public void DecreaseHappinessRatePercentage(float percentage)
    {
        _currentHappinessRate -= percentage;
        OnHappinessRateChange?.Invoke(); 
    }

    private void UpdateUI()
    {
        _currentHappinessRateTXT.text = $"{_currentHappinessRate}%";

        if (0 <= _currentHappinessRate && _currentHappinessRate <= 25)
        {
            UpdateImages(0);
        }
        else if (25 < _currentHappinessRate && _currentHappinessRate <= 50)
        {
            UpdateImages(1);
        }

        else if (50 < _currentHappinessRate && _currentHappinessRate <= 60)
        {
            UpdateImages(2);
        }

        else if(60 < _currentHappinessRate && _currentHappinessRate <= 70)
        {
            UpdateImages(3);
        }
        else if (70 < _currentHappinessRate && _currentHappinessRate <= 90)
        {
            UpdateImages(4);
        }
        else if (90 < _currentHappinessRate && _currentHappinessRate <= 100)
        {
            UpdateImages(5);
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
