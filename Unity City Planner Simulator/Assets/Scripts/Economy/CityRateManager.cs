using UnityEngine;
using System.Collections.Generic;

public class CityRateManager : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> _rateManagers = new List<MonoBehaviour>();
    private List<ICityRate> _rates = new List<ICityRate>();

    private void Awake()
    {
        foreach (var rateManager in _rateManagers)
        {
            if (rateManager is ICityRate rate)
            {
                _rates.Add(rate);
            }
        }
    }

    public void UpdateAllRates()
    {
        foreach (var rate in _rates)
        {
            rate.CalculateRate();
        }
    }
}
