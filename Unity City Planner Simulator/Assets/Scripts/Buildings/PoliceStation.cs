using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PoliceStation : Building
{
    [SerializeField] private List<GameObject> policePrefabs;
    private void Awake()
    {
    }
    public override void OnPlaced()
    {
        base.OnPlaced();
        if (State != BuildingState.Active) return;

        int policePrefabIndex = Random.Range(0, policePrefabs.Count);
        if (OccupiedPositions != null && OccupiedPositions.Count > 0)
        {
            ResidentsManager.Instance.SpawnPolicemans(1, (Vector3Int)OccupiedPositions[0], policePrefabs[policePrefabIndex]);
        }
        else
        {
            Debug.Log("There is no occupied positions");
        }
    }
}
