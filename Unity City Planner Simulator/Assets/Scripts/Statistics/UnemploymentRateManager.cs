using System;
using TMPro;
using UnityEngine;

public class UnemploymentRateManager : MonoBehaviour, ICityRate
{
    private float _unemploymentRate;
    [SerializeField] private TMP_Text currentUnemploymentRateTXT;

    public event Action OnUnemploymentRateChange;
    public float UnemploymentRate { get { return _unemploymentRate; } }
    public static UnemploymentRateManager Instance { get; private set; }
    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        OnUnemploymentRateChange += UpdateUI;
    }
    public void IncreaseRate(float rate)
    {
        _unemploymentRate = Mathf.Clamp(_unemploymentRate + rate, 0, 100);
        OnUnemploymentRateChange?.Invoke();
    }

    public void DecreaseRate(float rate)
    {
        _unemploymentRate = Mathf.Clamp(_unemploymentRate - rate, 0, 100);
        OnUnemploymentRateChange?.Invoke();
    }

    public void CalculateRate()
    {
        float laborForce = PopulationRateManager.Instance.CurrentPopulationRate;
        int jobsAvailable = 0;

        foreach (var building in EconomyManager.Instance.RegisteredBuildings)
        {
            IEmploymentProvider employmentProvider = building as IEmploymentProvider;
            if (employmentProvider != null)
            {
                jobsAvailable += employmentProvider.GetAvailableJobs();
            }
        }

        if (laborForce > 0)
        {
            _unemploymentRate = Mathf.Max(0, (laborForce - jobsAvailable) / laborForce * 100);
        }
        else
        {
            _unemploymentRate = 0;
        }
        UpdateUI();
        // max(0, (laborForce - jobsAvailable) / laborForce) * 100
    }

    private void UpdateUI()
    {
        currentUnemploymentRateTXT.text= _unemploymentRate.ToString();
    }
}
