using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class ZoneManager : MonoBehaviour
{
    private List<CityZone> _cityZones = new List<CityZone>();
    private bool showTiles = false;

    public static ZoneManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        bool shouldShowTiles = GridCity.Instance.GetActiveBuildingType() != null ||
                               BuildingMover.Instance.CurrentlyMovingBuilding != null;

        if (shouldShowTiles && !showTiles)
        {
            Debug.Log("Showing tiles");
            BuildingType currentType = GridCity.Instance.GetActiveBuildingType()?.buildingType ??
                                       BuildingMover.Instance.CurrentlyMovingBuilding.buildingData.buildingType;

            foreach (var zone in _cityZones.Where(z => z.zoneTypes.Contains(currentType)))
            {
                foreach (var tile in zone._visualTiles)
                {
                    var renderer = tile.GetComponent<SpriteRenderer>();
                    renderer.enabled = true;

                    if (!tile.TryGetComponent<TileVisualEffect>(out var effect))
                    {
                        effect = tile.AddComponent<TileVisualEffect>();
                    }
                }
            }
            showTiles = true;
            Debug.Log("Showing zone tiles.");
        }
        else if (!shouldShowTiles && showTiles)
        {
            foreach (var zone in _cityZones)
            {
                foreach (var tile in zone._visualTiles)
                {
                    if (tile.TryGetComponent<TileVisualEffect>(out var effect))
                    {
                        effect.ResetEffect();
                    }
                    tile.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            showTiles = false;
            Debug.Log("Hiding zone tiles.");
        }

        Debug.Log($"Current number of city zones: {_cityZones.Count}");
    }

    public void RegisterZone(CityZone zone)
    {
        if (!_cityZones.Contains(zone))
        {
            _cityZones.Add(zone);
        }
    }

    public void UnregisterZone(CityZone zone)
    {
        if (_cityZones.Contains(zone))
        {
            _cityZones.Remove(zone);
        }
    }

    public void RegisterBuilding(Building building)
    {
        foreach (CityZone zone in _cityZones)
        {
            zone.RegisterBuilding(building);
        }
    }

    public void ProcessTick()
    {
        foreach (var zone in _cityZones)
        {
            zone.ProcessTick();
        }
    }

    public List<CityZone> GetZonesContainingPosition(Vector2Int position)
    {
        List<CityZone> zones = new List<CityZone>();

        foreach (var zone in _cityZones)
        {
            if (zone.ContainsPosition(position))
            {
                zones.Add(zone);
            }
        }
        return zones;
    }
}
