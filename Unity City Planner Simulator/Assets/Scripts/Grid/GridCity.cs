using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridCity : MonoBehaviour
{
    private Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();
    private List<Vector2Int> obstacles = new List<Vector2Int>();

    public BuildingData selectedBuilding;

    [SerializeField] Grid grid;
    [SerializeField] Tilemap buildingTilemap;


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


            if (CanPlaceBuilding(gridPosition, selectedBuilding.size))
            {
                PlaceBuilding(selectedBuilding, gridPosition);
            }
        }
    }

    public bool CanPlaceBuilding(Vector2Int position, Vector2Int size)
    {
        for (int x = position.x; x < position.x + size.x; x++)
        {
            for (int y = position.y; y < position.y + size.y; y++)
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

    }
}
