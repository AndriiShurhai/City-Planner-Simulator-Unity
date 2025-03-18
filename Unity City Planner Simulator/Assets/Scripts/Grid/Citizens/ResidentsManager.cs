using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResidentsManager : MonoBehaviour
{
    [SerializeField] public Tilemap groundTilemap;
    [SerializeField] public Tilemap roadTilemap;

    [SerializeField] private List<GameObject> citizensPrefabs = new List<GameObject>();

    private bool isResidentSpawned = false;

    private float crimeCheckInterval = 40f;
    private float crimeCheckTimer = 0f;

    private List<AIResident> residents = new List<AIResident>();
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
    public void SpawnResidents(int amount, Vector3Int position)
    {
        isResidentSpawned = true;
        int offset = 1;

        position = new Vector3Int(position.x - offset * (amount / 2), position.y - offset, position.z);
        for (int i = 0; i < amount; i++)
        {
            int randomCitizenSpriteIndex = Random.Range(0, citizensPrefabs.Count - 1);

            GameObject prefabCitizen = citizensPrefabs[randomCitizenSpriteIndex];


            GameObject citizenInstance = Instantiate(prefabCitizen,
                position,
                Quaternion.identity
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

        foreach (var building in EconomyManager.Instance.registeredBuildings)
        {
            if (building.buildingData.buildingType == BuildingType.Commercial)
            {
                target = building;
                break;
            }
        }

        if (target == null) return;

        if (EconomyManager.Instance.registeredResidents.Count > 0 && residents.Count > 0)
        {
            residents[0].InitiateCrime(target);
        }
    }
}
