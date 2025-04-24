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

    private Dictionary<BuildingType, Dictionary<Vector2Int, int>> _typeToPositions = new Dictionary<BuildingType, Dictionary<Vector2Int, int>>();
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

        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
        {
            _typeToPositions[type] = new Dictionary<Vector2Int, int>();
            _typeToZones[type] = new List<CityZone>();
        }
    }

    private void Start()
    {
        Building.OnBuildingPlaced += HandleBuildingPlaced;
        Building.OnBuildingDestroyed += OnBuildingDestroyed;

        if (BuildingMover.Instance != null)
        {
            BuildingMover.Instance.OnBuildingStartMove += HandleBuildingStartMove;
        }

        if (GridCity.Instance != null)
        {
            GridCity.Instance.OnBuildingMoved += HandleBuildingMoved;
        }

        foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
        {
            UpdateBuildingPositions(building);
        }

        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
        {
            ProcessZoningForType(type);
        }
    }

    private void OnDestroy()
    {
        Building.OnBuildingPlaced -= HandleBuildingPlaced;
        Building.OnBuildingDestroyed -= OnBuildingDestroyed;

        if (BuildingMover.Instance != null)
        {
            BuildingMover.Instance.OnBuildingStartMove -= HandleBuildingStartMove;
        }

        if (GridCity.Instance != null)
        {
            GridCity.Instance.OnBuildingMoved -= HandleBuildingMoved;
        }
    }

    private void HandleBuildingStartMove(Building building)
    {
    }

    private void HandleBuildingMoved(Building building)
    {
        UpdateBuildingPositions(building);
        ProcessZoningForType(building.BuildingData.buildingType);

        foreach (var zone in _typeToZones[building.BuildingData.buildingType])
        {
            zone.RegisterBuilding(building);
        }
    }

    private void HandleBuildingPlaced(Building building)
    {
        UpdateBuildingPositions(building);
        ProcessZoningForType(building.BuildingData.buildingType);
    }

    private void UpdateBuildingPositions(Building building)
    {
        var type = building.BuildingData.buildingType;

        if (_buildingToPositions.TryGetValue(building, out var oldPositions))
        {
            foreach (var position in oldPositions)
            {
                if (_typeToPositions[type].ContainsKey(position))
                {
                    _typeToPositions[type][position]--;
                    if (_typeToPositions[type][position] <= 0)
                    {
                        _typeToPositions[type].Remove(position);
                    }
                }
            }
        }

        var newPositions = new List<Vector2Int>();
        foreach (var cell in building.OccupiedPositions)
        {
            for (int dx = -paddingSize; dx <= paddingSize; dx++)
            {
                for (int dy = -paddingSize; dy <= paddingSize; dy++)
                {
                    var p = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (!newPositions.Contains(p))
                    {
                        newPositions.Add(p);
                    }
                }
            }
        }

        foreach (var position in newPositions)
        {
            if (_typeToPositions[type].ContainsKey(position))
            {
                _typeToPositions[type][position]++;
            }
            else
            {
                _typeToPositions[type][position] = 1;
            }
        }

        _buildingToPositions[building] = newPositions;
    }

    private void ProcessZoningForType(BuildingType type)
    {
        List<Vector2Int> positions = _typeToPositions[type].Keys.ToList();

        if (positions.Count == 0)
        {
            foreach (var zone in _typeToZones[type])
            {
                ZoneManager.Instance.UnregisterZone(zone);
                try
                {
                    Destroy(zone.gameObject);
                }
                catch { }
            }
            _typeToZones[type].Clear();
            return;
        }

        List<List<Vector2Int>> clusters = FindClusters(positions);
        var handledZones = new HashSet<CityZone>();

        foreach (var cluster in clusters)
        {
            if (cluster.Count < minBuildingsForZone) continue;

            CityZone matchingZone = null;
            foreach (var zone in _typeToZones[type])
            {
                if (cluster.Any(pos => zone.ContainsPosition(pos)))
                {
                    matchingZone = zone;
                    break;
                }
            }

            if (matchingZone != null)
            {
                matchingZone.DefineZoneArea(cluster);
                handledZones.Add(matchingZone);
            }
            else
            {
                CreateZoneForCluster(type, cluster);
                handledZones.Add(_typeToZones[type].Last());
            }
        }

        var toRemove = _typeToZones[type].Except(handledZones).ToList();
        foreach (var zone in toRemove)
        {
            _typeToZones[type].Remove(zone);
            ZoneManager.Instance.UnregisterZone(zone);
            try
            {
                Destroy(zone.gameObject);
            }
            catch { }
        }
    }

    private List<List<Vector2Int>> FindClusters(List<Vector2Int> positions)
    {
        List<List<Vector2Int>> clusters = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (var pos in positions)
        {
            if (visited.Contains(pos)) continue;

            List<Vector2Int> cluster = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            queue.Enqueue(pos);
            visited.Add(pos);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                cluster.Add(current);

                foreach (var other in positions)
                {
                    if (!visited.Contains(other)
                        && Mathf.Abs(other.x - current.x) + Mathf.Abs(other.y - current.y) <= maxSearchDistance)
                    {
                        queue.Enqueue(other);
                        visited.Add(other);
                    }
                }
            }

            clusters.Add(cluster);

        }

        return clusters;
    }

    private void CreateZoneForCluster(BuildingType type, List<Vector2Int> cluster)
    {
        GameObject zoneObject = Instantiate(zonePrefab);
        zoneObject.name = $"Auto_{type}Zone_{System.DateTime.Now.Ticks}";

        CityZone zone = zoneObject.GetComponent<CityZone>();
        if (zone != null)
        {
            SetZoneColor(zone, type);

            zone.DefineZoneArea(cluster);

            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.RegisterZone(zone);
            }

            _typeToZones[type].Add(zone);

            zone.RegisterBuildingsInZone();

            Debug.Log($"Created new {type} zone with {cluster.Count} cells");
        }
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
        BuildingType type = building.BuildingData.buildingType;

        if (_buildingToPositions.TryGetValue(building, out var positions))
        {
            foreach (var position in positions)
            {
                if (_typeToPositions[type].ContainsKey(position))
                {
                    _typeToPositions[type][position]--;
                    if (_typeToPositions[type][position] <= 0)
                    {
                        _typeToPositions[type].Remove(position);
                    }
                }
            }

            _buildingToPositions.Remove(building);

            ProcessZoningForType(type);
        }
    }
}