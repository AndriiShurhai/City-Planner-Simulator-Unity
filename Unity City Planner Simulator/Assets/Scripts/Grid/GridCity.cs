using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridCity : MonoBehaviour
{
    private Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();
    private List<Vector2Int> obstacles = new List<Vector2Int>();

    [SerializeField] Grid grid;
    [SerializeField] Tilemap buildingTilemap;
    [SerializeField] private Building currentBuilding;

    private void Start()
    {
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(cellPosition.x, cellPosition.y);

            if (CanPlaceBuilding(gridPosition, currentBuilding))
            {
                PlaceBuilding(currentBuilding, gridPosition);
            }
        }
    }

    public bool CanPlaceBuilding(Vector2Int position, Building building)
    {
        for (int x = position.x; x < position.x + building.size.x; x++)
        {
            for (int y = position.y; y < position.y + building.size.y; y++)
            {
                Vector2Int checkPosition = new Vector2Int(x, y);
                if (buildings.ContainsKey(checkPosition) || obstacles.Contains(checkPosition))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void PlaceBuilding(Building building, Vector2Int position)
    {
        if (!CanPlaceBuilding(position, building))
        {
            return;
        }

        building.gridPosition = position;
        for (int x = position.x; x < position.x + building.size.x; x++)
        {
            for (int y = position.y; y < position.y + building.size.y; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                buildings[gridPos] = building;
            }
        }

        Vector3 worldPosition = grid.CellToWorld(new Vector3Int(position.x, position.y, 0));
        Transform buildingInstance = Instantiate(building.buildingData.buildingPrefab, worldPosition, Quaternion.identity, buildingTilemap.transform);
        building.OnPlaced();
    }


}
