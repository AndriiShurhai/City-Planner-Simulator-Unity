using UnityEngine;
using UnityEngine.Rendering;

public abstract class EntertainmentBuildingBase : Building, IHappinessProvider
{
    [SerializeField] protected float happinessBonus = 1f;

    protected override void OnProcessTick()
    {
        HappinessRateManager.Instance.IncreaseRate(happinessBonus);
    }

    protected override void OnUpgraded()
    {
        happinessBonus += 0.5f;
    }

    public float GetHappinessContribution() => happinessBonus;
}
