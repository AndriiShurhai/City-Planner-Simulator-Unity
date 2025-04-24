using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private List<Vector2Int> positionsForMovingBuilding = new List<Vector2Int>();
    private List<Vector2Int> _positionsForMoving;


    private void Awake()
    {
        zoneTypes ??= new List<BuildingType>();
    }

    private void Start()
    {
        if (BuildingMover.Instance != null)
        {
            BuildingMover.Instance.OnBuildingStartMove += HandleMovingBuilding;
        }

        if (GridCity.Instance != null)
        {
            GridCity.Instance.OnBuildingMoved += HandleMovedBuilding;
        }
    }
    private void OnDestroy()
    {
        if (BuildingMover.Instance != null)
            BuildingMover.Instance.OnBuildingStartMove -= HandleMovingBuilding;
        if (GridCity.Instance != null)
            GridCity.Instance.OnBuildingMoved -= HandleMovedBuilding;

        ClearVisualRepresentation();
    }

    public void DefineZoneArea(List<Vector2Int> gridPosition)
    {
        _zoneGridPosition.Clear();
        _zoneGridPosition = new HashSet<Vector2Int>(gridPosition);
        CreateVisualRepresentation();
        UpdateVisualsColor();
    }

    private void CreateVisualRepresentation()
    {
        if (this == null || gameObject == null) return;

        ClearVisualRepresentation();

        foreach (var pos in _zoneGridPosition)
        {
            Vector3 worldPosition = GridCity.Instance != null && GridCity.Instance.Grid != null
                ? GridCity.Instance.Grid.CellToWorld(new Vector3Int(pos.x, pos.y, 0))
                : new Vector3(pos.x, pos.y, 0);

            if (GridCity.Instance == null || GridCity.Instance.Grid == null)
                Debug.LogWarning("GridCity.Instance or Grid is null. Using fallback position.");

            if (tilePrefab == null)
            {
                Debug.LogWarning("tilePrefab is null in CityZone. Please assign a sprite prefab.");
                continue; // Skip tile creation instead of using a cube
            }

            GameObject tile = Instantiate(tilePrefab, transform);
            tile.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0);
            tile.transform.localScale = new Vector3(1, 1, 0.1f);

            if (!tile.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                renderer = tile.AddComponent<SpriteRenderer>();
                renderer.color = zoneColor;
            }
            else
            {
                renderer.color = zoneColor;
            }

            _visualTiles.Add(tile);
        }
    }

    private void ClearVisualRepresentation()
    {
        foreach (var tile in _visualTiles)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
        _visualTiles.Clear();
    }


    public bool ContainsPosition(Vector2Int position)
    {
        return _zoneGridPosition.Contains(position);
    }

    public void RegisterBuilding(Building building)
    {
        bool isInZone = false;

        foreach (var pos in building.OccupiedPositions)
        {
            if (_zoneGridPosition.Contains(pos))
            {
                isInZone = true;
                break;
            }
        }

        if (!isInZone)
        {
            return;
        }

        BuildingType type = building.BuildingData.buildingType;

        if (!_buildingsByType.ContainsKey(type))
        {
            _buildingsByType[type] = new List<Building>();
        }

        if (!_buildingsByType[type].Contains(building))
        {
            _buildingsByType[type].Add(building);

            if (!zoneTypes.Contains(type))
            {
                zoneTypes.Add(type);
            }

            Building.OnBuildingDestroyed += OnBuildingDestroyed;

            EvaluateZoneEffects();
        }
    }


    private void OnBuildingDestroyed(Building building)
    {
        var type = building.BuildingData.buildingType;
        if (_buildingsByType.TryGetValue(type, out var list))
        {
            list.Remove(building);
            Building.OnBuildingDestroyed -= OnBuildingDestroyed;
            if (list.Count == 0) zoneTypes.Remove(type);
            bool anyLeft = false;
            foreach (var kv in _buildingsByType)
                if (kv.Value.Count > 0) { anyLeft = true; break; }
            if (!anyLeft)
            {
                ZoneManager.Instance.UnregisterZone(this);
                try
                {
                    Destroy(gameObject);
                }
                catch { }
                return;
            }
            EvaluateZoneEffects();
        }
    }

    private void HandleMovingBuilding(Building building)
    {
        _positionsForMoving = new List<Vector2Int>(building.OccupiedPositions);
    }

    private void HandleMovedBuilding(Building building)
    {
        var type = building.BuildingData.buildingType;
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
    public int GetBuildingCount(BuildingType buildingType)
    {
        if (_buildingsByType.TryGetValue(buildingType, out List<Building> buildings))
        {
            return buildings.Count;
        }
        return 0;
    }

    private void EvaluateZoneEffects()
    {
        foreach (var effect in _activeZoneEffects)
        {
            effect.RemoveEffect();
        }
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
            effect.ApplyEffect();
            _activeZoneEffects.Add(effect);

            Debug.Log("Amusement distric effect");
        }
    }

    private void EvaluateResidentialDistrictEffect()
    {
        int residentialCount = GetBuildingCount(BuildingType.Residential);

        if (residentialCount > 5)
        {
            ZoneEffect effect = new ResidentialDistrictEffect(residentialCount);
            effect.ApplyEffect();
            _activeZoneEffects.Add(effect);

            Debug.Log("ResidentialDistric effect");
        }
    }

    private void EvaluateHealthcareDistrictEffect()
    {
        int medicalCount = GetBuildingCount(BuildingType.Medical);

        if (medicalCount > 2)
        {
            ZoneEffect effect = new HealthCareDistrictEffect(medicalCount);
            effect.ApplyEffect();
            _activeZoneEffects.Add(effect);
        }
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
        foreach (var tile in _visualTiles)
        {
            if (tile == null) continue;

            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Debug.Log($"Changing renderer color: prev {renderer.color}; new: {zoneColor}");
                renderer.color = zoneColor;
                renderer.enabled = false;
            }
            else
            {
                MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    if (meshRenderer.material == null)
                    {
                        meshRenderer.material = new Material(Shader.Find("Standard"));
                    }
                    meshRenderer.material.color = zoneColor;
                }
            }
        }
    }

    public List<Vector2Int> GetAllPositions()
    {
        return new List<Vector2Int>(_zoneGridPosition);
    }

    public void AddPositions(List<Vector2Int> newPositions)
    {
        bool changed = false;

        foreach (var pos in newPositions)
        {
            if (!_zoneGridPosition.Contains(pos))
            {
                _zoneGridPosition.Add(pos);
                changed = true;
            }
        }

        if (changed)
        {
            CreateVisualRepresentation();
        }
    }
    public void RegisterBuildingsInZone()
    {
        foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
        {
            RegisterBuilding(building);
        }
    }
}