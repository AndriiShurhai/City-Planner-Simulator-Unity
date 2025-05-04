using UnityEngine;
using System.Collections.Generic;
using System;

public class HospitalBuilding : ServiceBuildingBase, IHealthProvider
{
    [SerializeField] private List<GameObject> doctorsPrefabs;
    [SerializeField] private float healthRateBonus = 1f;

    private int currentDoctors;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        currentDoctors = 1;
    }

    protected override void ApplyServiceEffect()
    {
        HealthRateManager.Instance.IncreaseRate(healthRateBonus);
    }

    protected override void AfterPlacement()
    {
        base.AfterPlacement();

        if (State != BuildingState.Active) return;

        SpawnDoctors();
    }

    private void SpawnDoctors()
    {
        if (OccupiedPositions != null && OccupiedPositions.Count > 0)
        {
            int doctorIndex = UnityEngine.Random.Range(0, doctorsPrefabs.Count);
            ResidentsManager.Instance.SpawnDoctors(currentDoctors, (Vector3Int)OccupiedPositions[0], doctorsPrefabs[doctorIndex]);
        }
    }

    protected override void OnUpgraded()
    {
        base.OnUpgraded();

        healthRateBonus += 0.5f;

        currentDoctors++;
        int doctorIndex = UnityEngine.Random.Range(0, doctorsPrefabs.Count);
        ResidentsManager.Instance.SpawnDoctors(1, (Vector3Int)OccupiedPositions[0], doctorsPrefabs[doctorIndex]);
    }

    public float GetHealthContribution() => healthRateBonus * currentDoctors;

    public void UpdateHealthEffect()
    {
    } 
}
