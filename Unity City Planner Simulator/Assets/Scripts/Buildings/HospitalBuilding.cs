using UnityEngine;
using System.Collections.Generic;

public class HospitalBuilding : Building
{
    [SerializeField] private List<GameObject> doctorsPrefabs;
    public override void Initialize(BuildingData buildingData, Vector2Int size)
    {
        base.Initialize(buildingData, size);
    }

    public override void ProcessTick()
    {
        base.ProcessTick();
    }

    private void Start()
    {
        int doctorIndex = Random.Range(0, doctorsPrefabs.Count);
        ResidentsManager.Instance.SpawnDoctors(1, (Vector3Int)OccupiedPositions[0], doctorsPrefabs[doctorIndex]);
    }
}
