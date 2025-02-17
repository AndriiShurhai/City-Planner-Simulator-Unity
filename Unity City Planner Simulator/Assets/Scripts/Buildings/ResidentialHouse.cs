using UnityEngine;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using JetBrains.Annotations;

public class ResidentialHouse : Building
{
    private const int START_RESIDENTS = 1;
    private const int MAX_RESIDENTS = 3;

    [SerializeField] private ParticleSystem ps;

    private int currentLevel;
    private int currentResidents;
    private bool isPlaying = false;

    public override void Initialize(BuildingData buildingData, Vector2Int size)
    {
        base.Initialize(buildingData, size);
        currentResidents = START_RESIDENTS;
        currentLevel = 1;

        AddBuildingEffect(new UpgradeBoostResidentialEffect(this));
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
    private void Update()
    {
        if (!isPlaying)
        {
            StartCoroutine(PlayParticlesWithDelay());
        }
    }

    
    private IEnumerator PlayParticlesWithDelay()
    {
        isPlaying = true;
        float delay = UnityEngine.Random.Range(1f, 5f);
        yield return new WaitForSeconds(delay);

        ps.Play();
        isPlaying = false;
    }

}
