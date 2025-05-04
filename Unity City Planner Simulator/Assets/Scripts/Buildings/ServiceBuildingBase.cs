using System.Collections.Generic;
using UnityEngine;



public abstract class ServiceBuildingBase : Building
{
    [SerializeField] protected float serviceEffectStrength = 1f;

    protected override void OnProcessTick()
    {
        ApplyServiceEffect();
    }

    protected abstract void ApplyServiceEffect();

    protected override void OnUpgraded()
    {
        serviceEffectStrength += 0.5f;
    }
}
