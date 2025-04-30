using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;

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
        bool shouldShowTiles = IsBuildingInteractionActive();

        if (shouldShowTiles != showTiles)
        {
            UpdateTileVisibility(shouldShowTiles);
            showTiles = shouldShowTiles;
        }
    }


    private bool IsBuildingInteractionActive()
    {
        return GridCity.Instance.GetActiveBuildingType() != null ||
               BuildingMover.Instance.CurrentlyMovingBuilding != null;
    }

    private void UpdateTileVisibility(bool show)
    {
        BuildingType? currentType = show ? GetCurrentBuildingType() : null;

        foreach (var zone in _cityZones)
        {
            bool shouldShowZone = show && currentType.HasValue && zone.zoneTypes.Contains(currentType.Value);
            foreach (var tile in zone._visualTiles)
            {
                var renderer = tile.GetComponent<SpriteRenderer>();
                renderer.enabled = shouldShowZone;

                if (shouldShowZone)
                {
                    renderer.enabled = true;
                    var effect = tile.GetComponent<TileVisualEffect>() ?? tile.AddComponent<TileVisualEffect>();
                    effect.enabled = true;
                }
                else
                {
                    renderer.enabled = false;
                    if (tile.TryGetComponent(out TileVisualEffect effect))
                    {
                        effect.ResetEffect();
                        effect.enabled = false;
                    }
                }
            }
        }
    }

    private BuildingType GetCurrentBuildingType()
    {
        return GridCity.Instance?.GetActiveBuildingType()?.buildingType ??
               BuildingMover.Instance.CurrentlyMovingBuilding.BuildingData.buildingType;
    }
    public void RegisterZone(CityZone zone) => _cityZones.AddIfNotContains(zone);

    public void UnregisterZone(CityZone zone) => _cityZones.Remove(zone);

    public void RegisterBuilding(Building building)
    {
        foreach (var zone in _cityZones) zone.RegisterBuilding(building);
    }

    public void ProcessTick()
    {
        foreach (var zone in _cityZones) zone.ProcessTick();
    }

    public List<CityZone> GetZonesContainingPosition(Vector2Int position)
    {
        return _cityZones.Where(zone => zone.ContainsPosition(position)).ToList();
    }
}

public static class ListExtensions
{
    public static void AddIfNotContains<T>(this List<T> list, T item)
    {
        if (!list.Contains(item)) list.Add(item);
    }
}