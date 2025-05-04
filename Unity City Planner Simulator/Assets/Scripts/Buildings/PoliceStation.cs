using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PoliceStation : ServiceBuildingBase
{
    [SerializeField] private List<GameObject> policePrefabs;
    [SerializeField] private float crimeSuppression = 1.0f;

    private int policeCount = 1;

    protected override void AfterPlacement()
    {
        base.AfterPlacement();

        if (State != BuildingState.Active) return;

        SpawnPolice();
    }

    private void SpawnPolice()
    {
        if (OccupiedPositions != null && OccupiedPositions.Count > 0)
        {
            int policePrefabIndex = UnityEngine.Random.Range(0, policePrefabs.Count);
            ResidentsManager.Instance.SpawnPolicemans(1, (Vector3Int)OccupiedPositions[0], policePrefabs[policePrefabIndex]);
        }
    }

    protected override void ApplyServiceEffect()
    {
        CrimeRateManager.Instance.DecreaseRate(crimeSuppression);
    }

    protected override void OnUpgraded()
    {
        base.OnUpgraded();

        crimeSuppression += 0.5f;

        policeCount++;
        int policePrefabIndex = UnityEngine.Random.Range(0, policePrefabs.Count);
        ResidentsManager.Instance.SpawnPolicemans(1, (Vector3Int)OccupiedPositions[0], policePrefabs[policePrefabIndex]);
    }
}
