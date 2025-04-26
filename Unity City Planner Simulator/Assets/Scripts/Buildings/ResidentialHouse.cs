using UnityEngine;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using JetBrains.Annotations;
using System;

public class ResidentialHouse : Building
{
    private const int START_RESIDENTS = 1;

    [SerializeField] private ParticleSystem ps;

    private int currentLevel;
    private int currentResidents;
    private bool isPlaying = false;

    public new event Action<int, Vector3> OnUpgrade;

    public override void Initialize(BuildingData buildingData, Vector2Int size)
    {
        base.Initialize(buildingData, size);
        currentResidents = START_RESIDENTS;
        currentLevel = 1;

        if (!isPlaying)
        {
            StartCoroutine(PlayParticlesWithDelay());
        }
    }

    public override int CalculateIncome()
    {

        int taxIncome = buildingData.incomePerResident * currentResidents;
        int netIncome = taxIncome - buildingData.maintenanceCost;

        return netIncome;
    }

    public override void ProcessTick()
    {
        base.ProcessTick();

        Debug.Log("This what you can do after base process tick");
        // additional affects

    }


    
    private IEnumerator PlayParticlesWithDelay()
    {
        while (true)
        {
            isPlaying = true;
            float delay = UnityEngine.Random.Range(1f, 80f);
            yield return new WaitForSeconds(delay);

            ps.Play();
            isPlaying = false;

            Debug.Log("Doing this");
        }
    }

    public override void OnPlaced()
    {
        base.OnPlaced();
        if (State != BuildingState.Active) return; 

        if (OccupiedPositions != null && OccupiedPositions.Count > 0)
        {
            ResidentsManager.Instance.SpawnResidents(currentResidents, (Vector3Int)OccupiedPositions[0]);
        }
        else
        {
            Debug.Log("There is no occupied positions");
        }
    }
    public override void Upgrade()
    {
        if (currentLevel >= 3) return;

        currentLevel++;
        currentResidents++;

        ResidentsManager.Instance.SpawnResidents(1, (Vector3Int)OccupiedPositions[0]);
    }

}
