using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResidentsManager : MonoBehaviour, ISaveable
{
    [SerializeField] public Tilemap groundTilemap;
    [SerializeField] public Tilemap roadTilemap;

    [SerializeField] private List<GameObject> citizensPrefabs = new List<GameObject>();
    [SerializeField] private GameObject residentsParent;

    [SerializeField] private GameObject doctorPrefab;
    [SerializeField] private GameObject policemanPrefab;

    private bool isResidentSpawned = false;

    private float crimeCheckInterval = 10f;
    private float crimeCheckTimer = 10f;
    private float healthAttackInterval = 40f;
    private float healthAttackTimer = 40f;

    public event Action<AIResident> OnResidentGoingToCrime;
    public event Action<AIResident> OnResidentGoingToDie;

    private List<AIResident> residents = new List<AIResident>();

    private static int nextID = 0;
    public static ResidentsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        SaveManager.Instance.Register(this);
    }

    private void Start()
    {
        if (isResidentSpawned || residents.Count > 0)
        {
            crimeCheckTimer = 2f;
            healthAttackTimer = 40f;
        }
    }

    private void Update()
    {
        if (!isResidentSpawned && residents.Count <= 0) return;

        crimeCheckTimer -= Time.deltaTime;
        healthAttackTimer -= Time.deltaTime;   

        if (crimeCheckTimer <= 0)
        {
            DoCrimeIfCrimeRateIsHigh();
            crimeCheckTimer = crimeCheckInterval;
        }

        if (healthAttackTimer <= 0)
        {
            healthAttackTimer = healthAttackInterval;

            DoHealthAttackIfHealthRateIsLow();
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
            residentAI.residentID = nextID++;
            residentAI.Initialize(groundTilemap, roadTilemap, citizenInstance.GetComponent<Animator>());

            residentAI.InitializeComponents();

            PoliceStationManager.Instance.policemans.Add(residentAI);

            residents.Add(residentAI);

            position = new Vector3Int(position.x + offset, position.y, position.z);

            EconomyManager.Instance.RegisterResident(citizenInstance);
            PopulationRateManager.Instance.IncreaseRate(1);
        }
    }

    public void SpawnDoctors(int amount, Vector3Int position, GameObject prefab)
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

            Doctor residentAI = citizenInstance.GetComponent<Doctor>();

            if (residentAI == null)
            {
                residentAI = citizenInstance.AddComponent<Doctor>();
            }
            residentAI.residentID = nextID++;
            residentAI.Initialize(groundTilemap, roadTilemap, citizenInstance.GetComponent<Animator>());

            residentAI.InitializeComponents();

            residents.Add(residentAI);

            HospitalsManager.Instance.doctors.Add(residentAI);

            position = new Vector3Int(position.x + offset, position.y, position.z);

            EconomyManager.Instance.RegisterResident(citizenInstance);
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
            residentAI.residentID = nextID++;
            residentAI.Initialize(groundTilemap, roadTilemap, citizenInstance.GetComponent<Animator>());
            residents.Add( residentAI );

            position = new Vector3Int(position.x + offset, position.y, position.z);

            EconomyManager.Instance.RegisterResident(citizenInstance);
            PopulationRateManager.Instance.IncreaseRate(1);
        }
    }

    public void DoCrimeIfCrimeRateIsHigh()
    {
        if (CrimeRateManager.Instance.CrimeRate <= 30) return;
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

        Debug.Log($"Markets count: {EconomyManager.Instance.RegisteredBuildings.Where(x => x.BuildingData.Type == BuildingType.Commercial).Count()}");
        foreach (var building in EconomyManager.Instance.RegisteredBuildings)
        {
            Vector3 distance = criminal.transform.position - building.transform.position;
            if (building.BuildingData.Type == BuildingType.Commercial)
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

    public void DoHealthAttackIfHealthRateIsLow()
    {
        if (HealthRateManager.Instance.HealthRate >= 50) return;
        AIResident residentWithHealthProblem = null;
        if (residents.Count > 0)
        {
            int randomResidentIndex = UnityEngine.Random.Range(0, residents.Count);
            
            residentWithHealthProblem = residents[randomResidentIndex];
        }

        if (residentWithHealthProblem == null) return;

        residentWithHealthProblem.InitiateHealthAttack();
        OnResidentGoingToDie?.Invoke(residentWithHealthProblem);

    }

    public void RemoveResident(AIResident resident)
    {
        residents.Remove(resident);
    }

    public void Save(SaveData data)
    {
        data.residents = new List<ResidentSaveData>();
        foreach (var resident in residents)
        {
            ResidentSaveData residentData = new ResidentSaveData
            {
                residentID = resident.residentID,
                position = resident.transform.position,
                isCommittingCrime = resident.isCommitingCrime,
                isHavingHeartAttack = resident.isHavingHeartAttack,
                healthTimer = resident.isHavingHeartAttack ? resident.HealthTimer : 0f,
                currentDestination = resident.MovementController?.GetCurrentDestination() ?? resident.transform.position,
                isMoving = resident.MovementController?.IsMoving() ?? false,
            };

            if (resident is Doctor doctor)
            {
                residentData.residentType = "Doctor";
                residentData.isTryingToCure = doctor.IsTryingToCure;
                residentData.illCitizenID = doctor.IllCitizenTarget != null ? doctor.IllCitizenTarget.GetComponent<AIResident>().residentID : -1;
                doctor.isHavingHeartAttack = false;
                residentData.isHavingHeartAttack = false;
            }

            else if (resident is Policeman policeman)
            {
                residentData.residentType = "Policeman";
                residentData.isChasing = policeman.IsChasing;
                residentData.criminalID = policeman.CriminalTarget != null ? policeman.CriminalTarget.GetComponent<AIResident>().residentID : -1;
                residentData.chasingRecalculationCooldown = policeman.ChasingRecalculationCooldown;
                    
            }

            else
            {
                residentData.residentType = "AIResident";
                residentData.prefabIndex = citizensPrefabs.FindIndex(p => p.name == resident.gameObject.name);
                residentData.isTryingToCure = false;
                residentData.illCitizenID = -1;
                residentData.isChasing = false;
                residentData.criminalID = -1;
                residentData.chasingRecalculationCooldown = 1f;
            }

            data.residents.Add(residentData);
        }
        data.currentCriminalID = PoliceStationManager.Instance.currentCriminal != null ? PoliceStationManager.Instance.currentCriminal.residentID : -1;
        data.currentIllResidentID = HospitalsManager.Instance.currentIllResident != null ? HospitalsManager.Instance.currentIllResident.residentID : -1;
    }

    public void Load(SaveData data)
    {
        foreach (var resident in residents)
        {
            Destroy(resident.gameObject);
        }
        residents.Clear();

        PoliceStationManager.Instance.policemans.Clear();
        HospitalsManager.Instance.doctors.Clear();

        foreach (var residentData in data.residents)
        {
            GameObject prefab = residentData.residentType switch
            {
                "Doctor" => doctorPrefab,
                "Policeman" => policemanPrefab,
                _ => residentData.prefabIndex >= 0 && residentData.prefabIndex < citizensPrefabs.Count ? citizensPrefabs[residentData.prefabIndex] : citizensPrefabs[0]
            };

            GameObject residentObj = Instantiate(prefab, residentData.position, Quaternion.identity, residentsParent.transform);
            AIResident resident = residentObj.GetComponent<AIResident>();
            resident.Initialize(groundTilemap, roadTilemap, residentObj.GetComponent<Animator>());
            resident.residentID = residentData.residentID;
            resident.isCommitingCrime = residentData.isCommittingCrime;
            resident.isHavingHeartAttack = residentData.isHavingHeartAttack;

            if (residentData.isHavingHeartAttack)
            {
                resident.HealthTimer = residentData.healthTimer;
                resident.SpriteRenderer.color = Color.blue;
            }
            else if (residentData.isCommittingCrime)
            {
                resident.SpriteRenderer.color = Color.red;
            }
            else
            {
                resident.SpriteRenderer.color = Color.white;
            }

            if (resident is Doctor doctor)
            {
                doctor.isTryingToCure = residentData.isTryingToCure;
                HospitalsManager.Instance.doctors.Add(doctor);
            }
            else if (resident is Policeman policeman)
            {
                policeman.isChasing = residentData.isChasing;
                policeman.ChasingRecalculationCooldown = residentData.chasingRecalculationCooldown;
                PoliceStationManager.Instance.policemans.Add(policeman);
            }

            if (residentData.isMoving && resident.MovementController != null)
            {
                resident.MovementController.SetDestination(residentData.currentDestination);
            }

            residents.Add(resident);
        }

        foreach (var residentData in data.residents)
        {
            AIResident resident = residents.Find(r => r.residentID == residentData.residentID);

            if (resident is Doctor doctor && residentData.illCitizenID != -1)
            {
                AIResident illCitizen = residents.Find(r => r.residentID == residentData.illCitizenID);
                if (illCitizen != null)
                {
                    doctor.GoHealCitizen(illCitizen.transform, illCitizen);
                }
            }
            else if (resident is Policeman policeman && residentData.criminalID != -1)
            {
                AIResident criminal = residents.Find(r => r.residentID == residentData.criminalID);
                if (criminal != null)
                {
                    policeman.StartChasingCriminal(criminal.transform, criminal);
                }
            }
        }

        PoliceStationManager.Instance.currentCriminal = residents.Find(r => r.residentID == data.currentCriminalID);
        HospitalsManager.Instance.currentIllResident = residents.Find(r => r.residentID == data.currentIllResidentID);

        if (residents.Count > 0)
        {
            nextID = residents.Max(r => r.residentID) + 1;
        }
    }
}
