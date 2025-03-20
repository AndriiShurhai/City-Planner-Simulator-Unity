using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PoliceStation : Building
{
    [SerializeField] private List<GameObject> policePrefabs;
    private void Awake()
    {
    }
    void Start()
    {
        ResidentsManager.Instance.SpawnPolicemans(1, (Vector3Int)occupiedPositions[0], policePrefabs[0]);
    }
}
