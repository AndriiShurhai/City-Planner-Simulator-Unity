using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GridCity : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap buildingTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private CustomBuildingCursor customBuildingCursor;

    private Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();
    private BuildingData _selectedBuilding;
    public static GridCity Instance { get; private set; }
    public BuildingData SelectedBuilding { get; }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Update()
    {
        if (_selectedBuilding == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(cellPosition.x, cellPosition.y);


            if (CanPlaceBuilding(gridPosition, _selectedBuilding.size, _selectedBuilding))
            {
                PlaceBuilding(_selectedBuilding, gridPosition);
            }
        }
    }

    public bool CanPlaceBuilding(Vector2Int position, Vector2Int size, BuildingData building)
    {
        if (building == null || size.x <= 0 || size.y <= 0) return false;

        for (int x = position.x; x < position.x + size.x; x++)
        {
            for (int y = position.y; y < position.y + size.y; y++)
            {
                Vector2Int checkPosition = new Vector2Int(x, y);

                if (buildings.ContainsKey(checkPosition) || IsObstacleHere(new Vector3Int(checkPosition.x, checkPosition.y, 0)))
                {
                    return false;
                }
            }
        }
        BoxCollider2D collider = building.buildingPrefab.GetComponent<BoxCollider2D>();
        if (collider == null) return true;

        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        Collider2D overlap = Physics2D.OverlapBox(
            worldPosition + (Vector3)collider.offset,
            collider.size,
            0);

        return overlap == null;
    }

    private bool IsObstacleHere(Vector3Int cellPosition)
    {
        ObstacleRemover obstacleRemover = ObstacleRemover.Instance;
        if (obstacleRemover == null) return false;


        return ((obstacleRemover.LargeObstacleTilemap != null && obstacleRemover.LargeObstacleTilemap.GetTile(cellPosition) != null) ||
                (obstacleRemover.MiddleObstacleTilemap != null && obstacleRemover.MiddleObstacleTilemap.GetTile(cellPosition) != null) ||
                (obstacleRemover.SmallObstacleTilemap != null && obstacleRemover.SmallObstacleTilemap.GetTile(cellPosition) != null) ||
                roadTilemap != null && roadTilemap.GetTile(cellPosition) != null);
    }
    private void PlaceBuilding(BuildingData data, Vector2Int position)
    {
        Vector3 worldPosition = grid.CellToWorld(new Vector3Int(position.x, position.y, 0));

        Transform instance = Instantiate(data.buildingPrefab, worldPosition, Quaternion.identity);
        Building building = instance.GetComponent<Building>();

        building.Initialize(data, data.size); 
        building.OnPlaced();

        for (int x = position.x; x < position.x + data.size.x; x++)
        {
            for (int y = position.y; y < position.y + data.size.y; y++)
            {
                buildings[new Vector2Int(x, y)] = building;
            }
        }

        EconomyManager.Instance.SubtractMoney(building.buildingData.cost);

        customBuildingCursor.ToggleCursor(false, null);
        _selectedBuilding = null;

    }

    public void RemoveBuilding(Building building, List<Vector2Int> occupiedPositions)
    {
        if (building == null || occupiedPositions == null) return;

        foreach(Vector2Int position in occupiedPositions)
        {
            if (buildings.ContainsKey(position) && buildings[position] == building)
            {
                buildings.Remove(position);
            }
        }
    }

    public Building GetBuildingAt(Vector2Int position)
    {
        buildings.TryGetValue(position, out Building building);
        return building;
    }

    public void SetActiveBuildingType(BuildingData data)
    {
        _selectedBuilding = data;
        customBuildingCursor.ToggleCursor(true, data);
    }

    public BuildingData GetActiveBuildingType()
    {
        return _selectedBuilding;
    }
}
