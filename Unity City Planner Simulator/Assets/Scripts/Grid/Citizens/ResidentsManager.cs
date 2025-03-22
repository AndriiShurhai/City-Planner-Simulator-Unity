using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResidentsManager : MonoBehaviour
{
    [SerializeField] public Tilemap groundTilemap;
    [SerializeField] public Tilemap roadTilemap;

    [SerializeField] private List<GameObject> citizensPrefabs = new List<GameObject>();
    [SerializeField] private GameObject residentsParent;

    private bool isResidentSpawned = false;

    private float crimeCheckInterval = 40f;
    private float crimeCheckTimer = 0f;

    public event Action<AIResident> OnResidentGoingToCrime;

    private List<AIResident> residents = new List<AIResident>();
    private List<Policeman> policemans = new List<Policeman>();

    public static ResidentsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (isResidentSpawned)
        {
            crimeCheckTimer = 2f;
        }
    }

    private void Update()
    {
        if (!isResidentSpawned) return;

        crimeCheckTimer -= Time.deltaTime;

        if (crimeCheckTimer <= 0)
        {
            DoCrime();
            crimeCheckTimer = crimeCheckInterval;
        }
    }

    public void SpawnPolicemans(int amount, Vector3Int position, GameObject prefab)
    {
        int offset = 1;

        position = new Vector3Int(position.x - offset * (amount / 2), position.y - offset, position.z);

        for (int i = 0; i < amount; i++)
        {
            GameObject citizenInstance = Instantiate(prefab,
                position,
                Quaternion.identity,
                residentsParent.transform
            );
            
            Policeman residentAI = citizenInstance.GetComponent<Policeman>();

            if (residentAI == null)
            {
                residentAI = citizenInstance.AddComponent<Policeman>();
            }

            residentAI.Initialize(groundTilemap, roadTilemap, citizenInstance.GetComponent<Animator>());

            residentAI.InitializeComponents();

            PoliceStationManager.Instance.policemans.Add(residentAI);

            position = new Vector3Int(position.x + offset, position.y, position.z);

            EconomyManager.Instance.registeredResidents.Add(citizenInstance);
            PopulationRateManager.Instance.IncreaseRate(1);
        }
    }
    public void SpawnResidents(int amount, Vector3Int position)
    {
        isResidentSpawned = true;
        int offset = 1;

        position = new Vector3Int(position.x - offset * (amount / 2), position.y - offset, position.z);
        for (int i = 0; i < amount; i++)
        {
            int randomCitizenSpriteIndex = UnityEngine.Random.Range(0, citizensPrefabs.Count);

            GameObject prefabCitizen = citizensPrefabs[randomCitizenSpriteIndex];


            GameObject citizenInstance = Instantiate(prefabCitizen,
                position,
                Quaternion.identity,
                residentsParent.transform
            );
            AIResident residentAI = citizenInstance.GetComponent<AIResident>();

            if (residentAI == null)
            {
                residentAI = citizenInstance.AddComponent<AIResident>();
            }

            residentAI.Initialize(groundTilemap, roadTilemap, citizenInstance.GetComponent<Animator>());
            residents.Add( residentAI );

            position = new Vector3Int(position.x + offset, position.y, position.z);



            EconomyManager.Instance.registeredResidents.Add(citizenInstance);
            PopulationRateManager.Instance.IncreaseRate(1);
        }
    }

    public void DoCrime()
    {
        Building target = null;
        float minDistance = int.MaxValue;
        AIResident criminal = null;


        if (residents.Count > 0)
        {
            int randomResidentIndex = UnityEngine.Random.Range(0,residents.Count);
            Debug.Log($"Resident for crime will be: {residents[randomResidentIndex]}");

            criminal = residents[randomResidentIndex];
        }
        if (criminal == null) return;

        Debug.Log($"Markets count: {EconomyManager.Instance.registeredBuildings.Where(x => x.buildingData.buildingType == BuildingType.Commercial).Count()}");
        foreach (var building in EconomyManager.Instance.registeredBuildings)
        {
            Vector3 distance = criminal.transform.position - building.transform.position;
            if (building.buildingData.buildingType == BuildingType.Commercial)
            {
                float cells = Mathf.Abs(distance.x) + Mathf.Abs(distance.y) + Mathf.Abs(distance.z);

                if (cells < minDistance)
                {
                    minDistance = cells;
                    target = building;
                }
            }
        }
        if (target == null) return;

        criminal.InitiateCrime(target);
        OnResidentGoingToCrime?.Invoke(criminal);
    }
}
