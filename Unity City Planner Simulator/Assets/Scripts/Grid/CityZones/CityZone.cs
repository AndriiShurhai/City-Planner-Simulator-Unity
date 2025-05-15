using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class CityZone : MonoBehaviour
{
    [SerializeField] private string zoneName;
    [SerializeField] private Color zoneColor = new Color(0.5f, 0.5f, 1f, 0.3f);
    [SerializeField] private GameObject tilePrefab;

    private HashSet<Vector2Int> _zoneGridPosition = new HashSet<Vector2Int>();
    public List<GameObject> _visualTiles = new List<GameObject>();

    private Dictionary<BuildingType, List<Building>> _buildingsByType = new Dictionary<BuildingType, List<Building>>();
    private List<ZoneEffect> _activeZoneEffects = new List<ZoneEffect>();

    public List<BuildingType> zoneTypes = new List<BuildingType>();

    private void Awake()
    {
        zoneTypes ??= new List<BuildingType>();
        Building.OnBuildingDestroyed += OnBuildingDestroyed;
    }

    private void Start()
    {
        if (BuildingMover.Instance != null) BuildingMover.Instance.OnBuildingStartMove += HandleMovingBuilding;
        if (GridCity.Instance != null) GridCity.Instance.OnBuildingMoved += HandleMovedBuilding;
    }
    private void OnDestroy()
    {
        Building.OnBuildingDestroyed -= OnBuildingDestroyed;

        if (BuildingMover.Instance != null) BuildingMover.Instance.OnBuildingStartMove -= HandleMovingBuilding;
        if (GridCity.Instance != null) GridCity.Instance.OnBuildingMoved -= HandleMovedBuilding;

        ClearVisualRepresentation();
    }

    public void DefineZoneArea(List<Vector2Int> gridPosition)
    {
        _zoneGridPosition = new HashSet<Vector2Int>(gridPosition);
        CreateVisualRepresentation();
        UpdateVisualsColor();
        _buildingsByType.Clear();
        zoneTypes.Clear();
        RegisterBuildingsInZone();
        EvaluateZoneEffects();
    }

    private void CreateVisualRepresentation()
    {
        if (!this || !gameObject) return;

        ClearVisualRepresentation();

        foreach (var position in _zoneGridPosition) CreateTile(position);

    }

    private void CreateTile(Vector2Int position)
    {
        var worldPosition = GetWorldPosition(position);
        if (tilePrefab == null) return;

        var tile = Instantiate(tilePrefab, transform);
        tile.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0);
        tile.transform.localScale = new Vector3(1, 1, 0.1f);

        var renderer = tile.GetComponent<SpriteRenderer>() ?? tile.AddComponent<SpriteRenderer>();
        renderer.color = zoneColor;
        renderer.enabled = false;
        _visualTiles.Add(tile);
    }

    private Vector3 GetWorldPosition(Vector2Int position)
    {
        if (GridCity.Instance?.Grid == null) return new Vector3(position.x, position.y, 0);

        return GridCity.Instance.Grid.CellToWorld(new Vector3Int(position.x, position.y, 0));
    }

    private void ClearVisualRepresentation()
    {
        foreach (var tile in _visualTiles) if (tile) Destroy(tile);
        _visualTiles.Clear();
    }

    public bool ContainsPosition(Vector2Int position) => _zoneGridPosition.Contains(position);

    public void RegisterBuilding(Building building)
    {

        if (!IsBuildingInZone(building)) return;

        var type = building.BuildingData.Type;
        EnsureBuildingListExists(type);

        if (_buildingsByType[type].Contains(building)) return;

        _buildingsByType[type].Add(building);
        zoneTypes.AddIfNotContains(type);
        EvaluateZoneEffects();
    }

    private bool IsBuildingInZone(Building building) => building.OccupiedPositions.Any(_zoneGridPosition.Contains);

    private void EnsureBuildingListExists(BuildingType type)
    {
        if (!_buildingsByType.ContainsKey(type)) _buildingsByType[type] = new List<Building>();
    }
    private void OnBuildingDestroyed(Building building)
    {
        foreach (var kv in _buildingsByType)
        {
            if (kv.Value.Remove(building))
            {
                if (kv.Value.Count == 0) zoneTypes.Remove(kv.Key);
                if (_buildingsByType.All(b => b.Value.Count == 0)) DestroyZone();
                else EvaluateZoneEffects();
                break;
            }
        }
    }

    private void DestroyZone()
    {
        ZoneManager.Instance?.UnregisterZone(this);
        if (this) Destroy(gameObject);
    }

    private void HandleMovingBuilding(Building building) { }

    private void HandleMovedBuilding(Building building)
    {
        var type = building.BuildingData.Type;
        if (_buildingsByType.TryGetValue(type, out var list) && list.Contains(building))
        {
            bool stillInZone = building.OccupiedPositions.Any(pos => _zoneGridPosition.Contains(pos));
            if (!stillInZone)
            {
                list.Remove(building);
                if (list.Count == 0) zoneTypes.Remove(type);
                EvaluateZoneEffects();
            }
        }
    }
    public int GetBuildingCount(BuildingType buildingType) => _buildingsByType.TryGetValue(buildingType,out var buildings) ? buildings.Count : 0;

    private void EvaluateZoneEffects()
    {
        foreach (var effect in _activeZoneEffects) effect.RemoveEffect();
        
        _activeZoneEffects.Clear();

        EvaluateAmusementDistrictEffect();
        EvaluateHealthcareDistrictEffect();
        EvaluateResidentialDistrictEffect();
    }

    private void EvaluateAmusementDistrictEffect()
    {
        int amusementCount = GetBuildingCount(BuildingType.Amusement);

        if (amusementCount > 3)
        {
            float bonusMultiplier = Mathf.Min(1 + (amusementCount - 3) * 0.1f, 1.5f);

            ZoneEffect effect = new AmusementDistrictEffect(bonusMultiplier);
            AddEffect(effect);
        }
    }

    private void EvaluateResidentialDistrictEffect()
    {
        int residentialCount = GetBuildingCount(BuildingType.Residential);

        if (residentialCount > 5) AddEffect(new ResidentialDistrictEffect(residentialCount));
    }

    private void EvaluateHealthcareDistrictEffect()
    {
        int medicalCount = GetBuildingCount(BuildingType.Medical);

        if (medicalCount > 2) AddEffect(new HealthCareDistrictEffect(medicalCount));
    }

    private void AddEffect(ZoneEffect effect)
    {
        effect.ApplyEffect();
        _activeZoneEffects.Add(effect);
    }
    public void ProcessTick()
    {
        foreach (var effect in _activeZoneEffects)
        {
            effect.ApplyEffect();
        }
    }

    public void SetZoneColor(Color color)
    {
        Debug.Log($"Setting zone color to {color}");
        zoneColor = color;
        Debug.Log($"Now zone color is {zoneColor}");
        UpdateVisualsColor();
    }

    private void UpdateVisualsColor()
    {
        foreach (var tile in _visualTiles.Where(t => t != null))
            if (tile.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                renderer.color = zoneColor;
                renderer.enabled = false;
            }
    }

    public List<Vector2Int> GetAllPositions() => _zoneGridPosition.ToList();

    public void AddPositions(List<Vector2Int> newPositions) => DefineZoneArea(newPositions);
    public void RegisterBuildingsInZone()
    {
        foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
            RegisterBuilding(building);
    }
}