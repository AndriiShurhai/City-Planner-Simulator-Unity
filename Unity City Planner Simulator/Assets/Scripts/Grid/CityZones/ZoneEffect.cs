using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ZoneEffect
{
    public abstract void ApplyEffect();
    public abstract void RemoveEffect();
    public void ProcessTick() { }
}


public class AmusementDistrictEffect : ZoneEffect
{
    private float _happinessMultiplier;
    private float _appliedBonus;

    public AmusementDistrictEffect(float happinessMultiplier)
    {
        _happinessMultiplier = happinessMultiplier;
    }

    public override void ApplyEffect()
    {
        _appliedBonus = 2f * _happinessMultiplier;
        HappinessRateManager.Instance.IncreaseRate(_appliedBonus);
    }

    public override void RemoveEffect()
    {
        HappinessRateManager.Instance.DecreaseRate(_appliedBonus); 
    }
}


public class ResidentialDistrictEffect : ZoneEffect
{
    private float _appliedBonus;
    private int _residentialCount;

    public ResidentialDistrictEffect(int residentialCount)
    {
        _residentialCount = residentialCount;
    }

    public override void ApplyEffect()
    {
        _appliedBonus = Mathf.Min(_residentialCount * 0.5f, 5f);
        HappinessRateManager.Instance.IncreaseRate(_appliedBonus);
    }

    public override void RemoveEffect()
    {
        HappinessRateManager.Instance.DecreaseRate(_appliedBonus);
    }
}

public class HealthCareDistrictEffect : ZoneEffect
{
    private float _appliedBonus;
    private int _healthcareCount;

    public HealthCareDistrictEffect(int healthcareCount)
    {
        _healthcareCount = healthcareCount;
    }

    public override void ApplyEffect()
    {
        _appliedBonus = _healthcareCount * 1.5f;
        HealthRateManager.Instance.IncreaseRate(_appliedBonus);
    }

    public override void RemoveEffect()
    {
        HealthRateManager.Instance.DecreaseRate((_appliedBonus));
    }
}