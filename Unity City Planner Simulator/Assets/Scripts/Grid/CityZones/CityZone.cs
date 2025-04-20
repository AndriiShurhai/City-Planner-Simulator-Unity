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

    private void Awake()
    {
        if (zoneTypes == null)
        {
            zoneTypes = new List<BuildingType>();
        }
    }

    public void DefineZoneArea(List<Vector2Int> gridPosition)
    {
        _zoneGridPosition = new HashSet<Vector2Int>(gridPosition);
        CreateVisualRepresentation();
        UpdateVisualsColor();
    }

    private void CreateVisualRepresentation()
    {
        ClearVisualRepresentation();

        foreach (var pos in _zoneGridPosition)
        {
            Vector3 worldPosition = Vector3.zero;

            if (GridCity.Instance != null && GridCity.Instance.Grid != null)
            {
                worldPosition = GridCity.Instance.Grid.CellToWorld(new Vector3Int(pos.x, pos.y, 0));
            }
            else
            {
                worldPosition = new Vector3(pos.x, pos.y, 0);
                Debug.LogWarning("GridCity.Instance or Grid is null. Using fallback position calculation.");
            }

            GameObject tile;

            if (tilePrefab != null)
            {
                tile = Instantiate(tilePrefab, transform);
            }
            else
            {
                tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.SetParent(transform);
            }

            tile.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0);
            tile.transform.localScale = new Vector3(1, 1, 0.1f);

            if (!tile.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                renderer = tile.AddComponent<SpriteRenderer>();
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

    private void OnDestroy()
    {
        ClearVisualRepresentation();
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

            building.OnBuildingDestroyed += OnBuildingDestroyed;

            EvaluateZoneEffects();
        }
    }

    private void OnBuildingDestroyed(Building building)
    {
        BuildingType type = building.BuildingData.buildingType;

        if (_buildingsByType.ContainsKey(type))
        {
            _buildingsByType[type].Remove(building);
            building.OnBuildingDestroyed -= OnBuildingDestroyed;

            if (_buildingsByType[type].Count == 0)
            {
                zoneTypes.Remove(type);
            }

            EvaluateZoneEffects();
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
        zoneColor = color;
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
                renderer.color = zoneColor;
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
}