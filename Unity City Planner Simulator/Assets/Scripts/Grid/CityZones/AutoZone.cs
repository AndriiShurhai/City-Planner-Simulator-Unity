using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AutoZoneManager : MonoBehaviour
{
    [SerializeField] private GameObject zonePrefab;
    [SerializeField] private Color[] zoneColorsByType;
    [SerializeField] private int minBuildingsForZone = 1;
    [SerializeField] private int maxSearchDistance = 10;
    [SerializeField] private int paddingSize = 2;

    private Dictionary<BuildingType, List<Vector2Int>> _typeToPositions = new Dictionary<BuildingType, List<Vector2Int>>();
    private Dictionary<BuildingType, List<CityZone>> _typeToZones = new Dictionary<BuildingType, List<CityZone>>();

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
            _typeToPositions[type] = new List<Vector2Int>();
            _typeToZones[type] = new List<CityZone>();
        }
    }

    private void Start()
    {
        Building.OnBuildingPlaced += HandleBuildingPlaced;

        foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
        {
            RegisterBuildingPositions(building);
        }

        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
        {
            ProcessZoningForType(type);
        }

        foreach (var zone in FindObjectsByType<CityZone>(FindObjectsSortMode.None))
        {
            foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
            {
                zone.RegisterBuilding(building);
            }
        }
    }

    private void OnDestroy()
    {
        Building.OnBuildingPlaced -= HandleBuildingPlaced;
    }

    private void HandleBuildingPlaced(Building building)
    {
        RegisterBuildingPositions(building);
        ProcessZoningForType(building.BuildingData.buildingType);
    }

    private void RegisterBuildingPositions(Building building)
    {
        BuildingType type = building.BuildingData.buildingType;

        for (int i = 0; i < building.OccupiedPositions.Count; i++)
        {
            Vector2Int position = building.OccupiedPositions[i];
            if (!_typeToPositions[type].Contains(position))
            {
                _typeToPositions[type].Add(position);
            }

            for (int dx = -paddingSize; dx <= paddingSize; dx++)
            {
                for (int dy = -paddingSize; dy <= paddingSize; dy++)
                {
                    position = new Vector2Int(building.OccupiedPositions[i].x + dx, building.OccupiedPositions[i].y + dy);

                    if (!_typeToPositions[type].Contains(position))
                    {
                        _typeToPositions[type].Add(position);
                    }
                }
            }
        }

        //foreach (var position in building.OccupiedPositions)
        //{
        //    if (!_typeToPositions[type].Contains(position))
        //    {
        //        _typeToPositions[type].Add(position);
        //        Debug.Log($"{position}");
        //    }
        //}
    }

    private void ProcessZoningForType(BuildingType type)
    {
        List<Vector2Int> positions = _typeToPositions[type];
        if (positions.Count < minBuildingsForZone) return;

        List<List<Vector2Int>> clusters = FindClusters(positions);

        // Process each cluster
        foreach (var cluster in clusters)
        {
            // Skip small clusters
            if (cluster.Count < minBuildingsForZone) continue;

            bool clusterHandled = false;

            // Check if cluster is already in a zone of this type
            foreach (var zone in _typeToZones[type])
            {
                bool shouldExtendZone = false;
                foreach (var pos in cluster)
                {
                    if (zone.ContainsPosition(pos))
                    {
                        shouldExtendZone = true;
                        break;
                    }
                }

                if (shouldExtendZone)
                {
                    // Extend existing zone
                    ExtendZone(zone, cluster);
                    clusterHandled = true;
                    break;
                }
            }

            if (!clusterHandled)
            {
                // Create a new zone for this cluster
                CreateZoneForCluster(type, cluster);
            }
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

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx != 0 && dy != 0) continue;

                        Vector2Int neighbor = new Vector2Int(current.x + dx, current.y + dy);

                        if (positions.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }

                for (int dx = -maxSearchDistance; dx <= maxSearchDistance; dx++)
                {
                    for (int dy = -maxSearchDistance; dy <= maxSearchDistance; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        Vector2Int checkPos = new Vector2Int(current.x + dx, current.y + dy);

                        if (positions.Contains(checkPos) && !visited.Contains(checkPos) &&
                            Mathf.Abs(dx) + Mathf.Abs(dy) <= maxSearchDistance)
                        {
                            queue.Enqueue(checkPos);
                            visited.Add(checkPos);
                        }
                    }
                }
            }

            if (cluster.Count > 0)
            {
                clusters.Add(cluster);
            }
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

            RegisterBuildingsToZone(zone);

            Debug.Log($"Created new {type} zone with {cluster.Count} cells");
        }
    }

    private void ExtendZone(CityZone zone, List<Vector2Int> newPositions)
    {
        List<Vector2Int> positionsToAdd = new List<Vector2Int>();

        foreach (var pos in newPositions)
        {
            if (!zone.ContainsPosition(pos))
            {
                positionsToAdd.Add(pos);
            }
        }

        if (positionsToAdd.Count == 0) return;

        List<Vector2Int> allPositions = zone.GetAllPositions();
        allPositions.AddRange(positionsToAdd);

        zone.DefineZoneArea(allPositions);

        Debug.Log($"Extended zone with {positionsToAdd.Count} new cells");
    }

    private void SetZoneColor(CityZone zone, BuildingType type)
    {
        int typeIndex = (int)type;
        if (zoneColorsByType != null && typeIndex < zoneColorsByType.Length)
        {
            zone.SetZoneColor(zoneColorsByType[typeIndex]);
        }
    }

    private void RegisterBuildingsToZone(CityZone zone)
    {
        foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
        {
            zone.RegisterBuilding(building);
        }
    }
}