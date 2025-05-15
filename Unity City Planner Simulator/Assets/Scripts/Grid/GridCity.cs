using System;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridCity : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap buildingTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private CustomBuildingCursor customBuildingCursor;

    private Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();
    private BuildingData _selectedBuilding;

    public Action<Building> OnBuildingMoved;
    public static GridCity Instance { get; private set; }
    public BuildingData SelectedBuilding { get;}

    public Grid Grid { get { return grid; } }

    public Dictionary<Vector2Int, Building> Buildings
    {
        get { return buildings; }
    }

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
        UpdateCursor();

        if (BuildingMover.Instance.CurrentlyMovingBuilding != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                BuildingMover.Instance.CancelBuildingMove();
            }
            else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPosition = grid.WorldToCell(mouseWorldPosition);
                Vector2Int gridPosition = new Vector2Int(cellPosition.x, cellPosition.y);

                if (CanPlaceBuilding(gridPosition, BuildingMover.Instance.CurrentlyMovingBuilding.BuildingData.Size,
                                     BuildingMover.Instance.CurrentlyMovingBuilding.BuildingData))
                {
                    MoveBuilding(BuildingMover.Instance.CurrentlyMovingBuilding, gridPosition);
                    BuildingMover.Instance.CurrentlyMovingBuilding = null;
                }
            }
        }

        else if ((_selectedBuilding != null && !EventSystem.current.IsPointerOverGameObject()))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
                Vector2Int gridPosition = new Vector2Int(cellPosition.x, cellPosition.y);


                if (CanPlaceBuilding(gridPosition, _selectedBuilding.Size, _selectedBuilding))
                {
                    PlaceBuilding(_selectedBuilding, gridPosition);
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                _selectedBuilding = null;
            }
        }

        else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(cellPosition.x, cellPosition.y);

            Building building = GetBuildingAt(gridPosition);

            if (building != null)
            {
                Debug.Log("Building clicked at: " + gridPosition);
                BuildingPanel.Instance.ShowBuildingPanel(building, building.OccupiedPositions[0] + new Vector2((float)building.BuildingData.Size.x / 2f, -2));
            }
            else
            {
                BuildingPanel.Instance.HideBuildingPanel();
            }
        }
    }

    private void UpdateCursor()
    {
        bool show = (GetActiveBuildingType() != null || BuildingMover.Instance.CurrentlyMovingBuilding != null) && !EventSystem.current.IsPointerOverGameObject();
        customBuildingCursor.ToggleCursor(show);
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
        BoxCollider2D collider = building.Prefab.GetComponent<BoxCollider2D>();
        if (collider == null) return true;

        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        Collider2D overlap = Physics2D.OverlapBox(
            new Vector3(position.x, position.y, 0) + (Vector3)collider.offset,
            collider.size,
            0
        );

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

        Transform instance = Instantiate(data.Prefab, worldPosition, Quaternion.identity);
        Building building = instance.GetComponent<Building>();

        building.Initialize(data, data.Size); 
        building.SetGridPosition(position);

        for (int x = position.x; x < position.x + data.Size.x; x++)
        {
            for (int y = position.y; y < position.y + data.Size.y; y++)
            {
                buildings[new Vector2Int(x, y)] = building;
            }
        }

        EconomyManager.Instance.SubtractMoney(building.BuildingData.Cost);
        customBuildingCursor.ToggleCursor(false, null);
        _selectedBuilding = null;
    }

    private void MoveBuilding(Building building, Vector2Int newPosition)
    {
        building.SetGridPosition(newPosition);

        for (int x = newPosition.x; x < newPosition.x + building.BuildingData.Size.x; x++)
        {
            for (int y = newPosition.y; y < newPosition.y + building.BuildingData.Size.y; y++)
            {
                buildings[new Vector2Int(x, y)] = building;
            }
        }

        Vector3 worldPosition = grid.CellToWorld(new Vector3Int(newPosition.x, newPosition.y, 0));
        building.transform.position = worldPosition;

        BuildingMover.Instance.ActivateCurrentlyMovingBuilding();
        OnBuildingMoved?.Invoke(building);
    }
    public void RemoveBuilding(Building building, List<Vector2Int> occupiedPositions)
    {
        if (building == null || occupiedPositions == null) return;

        Debug.Log("Trying to remove a building in GridCity class");

        foreach (Vector2Int position in occupiedPositions)
        {
            if (buildings.ContainsKey(position) && buildings[position] == building)
            {
                buildings.Remove(position);
            }
        }
    }

    public Building GetBuildingAt(Vector2Int position)
    {
        if (buildings.TryGetValue(position, out Building building))
        {
            return building;
        }
        return null;
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
