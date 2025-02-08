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
    private Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();
    private List<Vector2Int> obstacles = new List<Vector2Int>();

    public BuildingData selectedBuilding;

    [SerializeField] Grid grid;
    [SerializeField] Tilemap buildingTilemap;
    [SerializeField] CustomBuildingCursor customBuildingCursor;

    public static GridCity Instance { get; private set; }

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
        if (selectedBuilding == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(cellPosition.x, cellPosition.y);


            if (CanPlaceBuilding(gridPosition, selectedBuilding.size, selectedBuilding) && !EventSystem.current.IsPointerOverGameObject())
            {
                PlaceBuilding(selectedBuilding, gridPosition);
            }
        }
    }

    public bool CanPlaceBuilding(Vector2Int position, Vector2Int size, BuildingData building)
    {
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
        return (ObstacleRemover.Instance.LargeObstacleTilemap.GetTile(cellPosition) != null || 
                ObstacleRemover.Instance.MiddleObstacleTilemap.GetTile(cellPosition) != null ||
                ObstacleRemover.Instance.SmallObstacleTilemap.GetTile(cellPosition) != null);
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
        selectedBuilding = null;
        customBuildingCursor.ToggleCursor(false, null);

    }

    public void SetActiveBuildingType(BuildingData data)
    {
        selectedBuilding = data;
        customBuildingCursor.ToggleCursor(true, data);
    }

    public BuildingData GetActiveBuildingType()
    {
        return selectedBuilding;
    }
}
