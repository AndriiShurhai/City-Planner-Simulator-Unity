using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AutoZoneManager : MonoBehaviour
{
    [SerializeField] private GameObject zonePrefab;
    [SerializeField] private Color[] zoneColorsByType;
    [SerializeField] private int minBuildingsForZone = 1;
    [SerializeField] private int maxSearchDistance = 10;
    [SerializeField] private int paddingSize = 2;

    private Dictionary<BuildingType, Dictionary<Vector2Int, int>> _influenceCountsByType = new Dictionary<BuildingType, Dictionary<Vector2Int, int>>();
    private Dictionary<BuildingType, List<CityZone>> _typeToZones = new Dictionary<BuildingType, List<CityZone>>();
    private Dictionary<Building, List<Vector2Int>> _buildingToPositions = new Dictionary<Building, List<Vector2Int>>();

    public static AutoZoneManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
        {
            _influenceCountsByType[type] = new Dictionary<Vector2Int, int>();
            _typeToZones[type] = new List<CityZone>();
        }
    }

    private void Start()
    {
        SubscribeToEvents();
        InitializeExistingBuildings();
        ProcessAllZoning();
    }

    private void SubscribeToEvents()
    {
        Building.OnBuildingPlaced += HandleBuildingPlaced;
        Building.OnBuildingDestroyed += OnBuildingDestroyed;
        if (BuildingMover.Instance != null) BuildingMover.Instance.OnBuildingStartMove += HandleBuildingStartMove;
        if (GridCity.Instance != null) GridCity.Instance.OnBuildingMoved += HandleBuildingMoved;
    }


    private void OnDestroy()
    {
        Building.OnBuildingPlaced -= HandleBuildingPlaced;
        Building.OnBuildingDestroyed -= OnBuildingDestroyed;

        if (BuildingMover.Instance != null) BuildingMover.Instance.OnBuildingStartMove -= HandleBuildingStartMove;
        if (GridCity.Instance != null) GridCity.Instance.OnBuildingMoved -= HandleBuildingMoved;
    }

    private void HandleBuildingStartMove(Building building) { }
    private void HandleBuildingMoved(Building building)
    {
        UpdateAndProcess(building);

        foreach (var zone in _typeToZones[building.BuildingData.buildingType])
        {
            zone.RegisterBuilding(building);
        }
    }
    private void HandleBuildingPlaced(Building building) => UpdateAndProcess(building);

    private void UpdateAndProcess(Building building)
    {
        if (building is not IZonable) return;
        Debug.Log("Building placed event received");
        UpdateBuildingPositions(building);
        ProcessZoningForType(building.BuildingData.buildingType);
    }

    private void UpdateBuildingPositions(Building building)
    {
        var type = building.BuildingData.buildingType;
        RemoveOldPositions(building, type);

        var newPositions = AddNewPositions(building, type);
        _buildingToPositions[building] = newPositions;
    }

    private void RemoveOldPositions(Building building, BuildingType type)
    {
        if (_buildingToPositions.TryGetValue(building, out var oldPositions))
        {
            foreach (var position in oldPositions)
            {
                if (_influenceCountsByType[type].TryGetValue(position, out int count))
                {
                    _influenceCountsByType[type][position] = count - 1;
                    if (_influenceCountsByType[type][position] <= 0)
                    {
                        _influenceCountsByType[type].Remove(position);
                    }
                }
            }
        }
    }

    private List<Vector2Int> AddNewPositions(Building building, BuildingType type)
    {
        var newPositions = new List<Vector2Int>();
        foreach (var cell in building.OccupiedPositions)
        {
            for (int dx = -paddingSize; dx <= paddingSize; dx++)
            {
                for (int dy = -paddingSize; dy <= paddingSize; dy++)
                {
                    var position = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (!newPositions.Contains(position)) newPositions.Add(position);
                }
            }
        }

        foreach (var position in newPositions)
        {
            _influenceCountsByType[type][position] = _influenceCountsByType[type].GetValueOrDefault(position) + 1;
        }

        return newPositions;
    }

    private void ProcessZoningForType(BuildingType type)
    {
        List<Vector2Int> positions = _influenceCountsByType[type].Keys.ToList();

        if (positions.Count == 0) { ClearZones(type); return; }

        var clusters = FindClusters(positions);
        var handledZones = UpdateOrCreateZones(type, clusters);
        RemoveUnusedZones(type, handledZones);
    }

    private void ClearZones(BuildingType type)
    {
        foreach (var zone in _typeToZones[type]) DestroyZone(zone);
        _typeToZones[type].Clear();
    }

    private HashSet<CityZone> UpdateOrCreateZones(BuildingType type, List<List<Vector2Int>> clusters)
    {
        var handledZones = new HashSet<CityZone>();
        foreach (var cluster in clusters.Where(c => c.Count >= minBuildingsForZone))
        {
            var zone = FindMatchingZone(type, cluster) ?? CreateZoneForCluster(type, cluster);
            zone.DefineZoneArea(cluster);
            handledZones.Add(zone);
        }

        return handledZones;
    }

    private CityZone FindMatchingZone(BuildingType type, List<Vector2Int> cluster)
    {
        return _typeToZones[type].FirstOrDefault(zone => cluster.Any(zone.ContainsPosition));
    }

    private void RemoveUnusedZones(BuildingType type, HashSet<CityZone> handledZones)
    {
        var toRemove = _typeToZones[type].Except(handledZones).ToList();
        foreach (var zone in toRemove)
        {
            _typeToZones[type].Remove(zone);
            DestroyZone(zone);
        }
    }
    private List<List<Vector2Int>> FindClusters(List<Vector2Int> positions)
    {
        List<List<Vector2Int>> clusters = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (var pos in positions.Where(p => !visited.Contains(p)))
        {
            List<Vector2Int> cluster = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            queue.Enqueue(pos);
            visited.Add(pos);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                cluster.Add(current);

                foreach (var other in positions.Where(o => !visited.Contains(o) && ManhattanDistance(current, o) <= maxSearchDistance))
                {
                    queue.Enqueue(other);
                    visited.Add(other);
                }
            }
            clusters.Add(cluster);
        }
        return clusters;
    }

    private int ManhattanDistance(Vector2Int current, Vector2Int other) => Mathf.Abs(current.x - other.x) + Mathf.Abs(current.y - other.y);

    private CityZone CreateZoneForCluster(BuildingType type, List<Vector2Int> cluster)
    {
        GameObject zoneObject = Instantiate(zonePrefab);
        zoneObject.name = $"Auto_{type}Zone_{System.DateTime.Now.Ticks}";
        var zone = zoneObject.GetComponent<CityZone>();
        Debug.Log($"Created zone {zoneObject.name} for type {type} with {cluster.Count} positions");
        SetZoneColor(zone, type);
        ZoneManager.Instance?.RegisterZone(zone);
        _typeToZones[type].Add(zone);
        zone.RegisterBuildingsInZone();
        return zone;
    }
    private void SetZoneColor(CityZone zone, BuildingType type)
    {
        int typeIndex = (int)type;
        if (zoneColorsByType != null && typeIndex < zoneColorsByType.Length)
        {
            zone.SetZoneColor(zoneColorsByType[typeIndex]);
        }
    }

    private void OnBuildingDestroyed(Building building)
    {
        RemoveOldPositions(building, building.BuildingData.buildingType);
        _buildingToPositions.Remove(building);
        ProcessZoningForType(building.BuildingData.buildingType);
    }

    private void DestroyZone(CityZone zone)
    {
        ZoneManager.Instance?.UnregisterZone(zone);
        if (zone != null) Destroy(zone.gameObject);
    }
    private void InitializeExistingBuildings()
    {
        foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
        {
            UpdateBuildingPositions(building);
        }
    }

    private void ProcessAllZoning()
    {
        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
        {
            ProcessZoningForType(type);
        }
    }
}