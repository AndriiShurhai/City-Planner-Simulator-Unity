using UnityEngine;
using System.Collections.Generic;

public class HospitalBuilding : Building
{
    [SerializeField] private List<GameObject> doctorsPrefabs;
    [SerializeField] private float healthrateBonus = 1f;
    private int currentDoctors;
    public override void Initialize(BuildingData buildingData, Vector2Int size)
    {
        base.Initialize(buildingData, size);
        currentDoctors = 1;
    }

    public override void ProcessTick()
    {
        base.ProcessTick();
        HealthRateManager.Instance.IncreaseRate(healthrateBonus);
    }

    public override void OnPlaced()
    {
        base.OnPlaced();
        if (State != BuildingState.Active) return;

        if (OccupiedPositions != null && OccupiedPositions.Count > 0)
        {
            int doctorIndex = Random.Range(0, doctorsPrefabs.Count);
            ResidentsManager.Instance.SpawnDoctors(currentDoctors, (Vector3Int)OccupiedPositions[0], doctorsPrefabs[doctorIndex]);
        }
        else
        {
            Debug.Log("There is no occupied positions");
        }
    }

    public override void Upgrade()
    {
        base.Upgrade();
        if (buildingData.upgradeLevel >= buildingData.maxUpgradeLevel)
        {
            return;
        }
        currentDoctors++;
        int doctorIndex = Random.Range(0, doctorsPrefabs.Count);
        ResidentsManager.Instance.SpawnDoctors(1, (Vector3Int)OccupiedPositions[0], doctorsPrefabs[doctorIndex]);
    }
}
