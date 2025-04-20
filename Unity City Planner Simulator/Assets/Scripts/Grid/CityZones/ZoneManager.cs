using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        if (GridCity.Instance.GetActiveBuildingType() == null && showTiles)
        {
            foreach (var zone in _cityZones)
            {
                foreach(var tile in zone._visualTiles)
                {

                    if (tile.TryGetComponent<TileVisualEffect>(out var effect))
                    {
                        effect.ResetEffect();
                        Destroy(effect);
                    }
                    tile.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            showTiles = false;
        }

        if (GridCity.Instance.GetActiveBuildingType() != null && !showTiles)
        {
            foreach( var zone in _cityZones)
            {
                if (zone.zoneTypes.Contains(GridCity.Instance.GetActiveBuildingType().buildingType))
                {
                    Debug.Log($"Zone neame: {zone.name}; Buildings in this zone: {zone.GetBuildingCount(GridCity.Instance.GetActiveBuildingType().buildingType)}");
                    foreach (var tile in zone._visualTiles)
                    {
                        tile.GetComponent<SpriteRenderer>().enabled = true;

                        var originalPosition = tile.transform.position;

                        if (!tile.TryGetComponent<TileVisualEffect>(out var effect))
                        {
                            effect = tile.gameObject.AddComponent<TileVisualEffect>();
                            effect.spriteRenderer = tile.GetComponent<SpriteRenderer>();
                            effect.originalPosition = tile.transform.position; // 
                            effect.baseColor = tile.GetComponent<SpriteRenderer>().color;
                            effect.timeOffset = Random.Range(0f, 2f);
                        }
                    }
                }
            }
            showTiles = true;
        }
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
