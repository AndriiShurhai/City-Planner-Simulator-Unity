using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingMover : MonoBehaviour
{
    public static BuildingMover Instance { get; private set; }

    private Building currentlyMovingBuilding;
    private Vector2Int originalGridPosition;

    public Building CurrentlyMovingBuilding { 
        get 
        {
            return currentlyMovingBuilding;
        }
        set
        {
            currentlyMovingBuilding = value;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool StartMovingBuilding(Building building)
    {
        if (building == null || GridCity.Instance == null)
        {
            return false;
        }

        currentlyMovingBuilding = building;
        originalGridPosition = building.GridPosition;

        GridCity.Instance.RemoveBuilding(building, building.OccupiedPositions);

        DisactivateCurrentlyMovingBuilding();

        return true;
    }

    public void CancelBuildingMove()
    {
        if (currentlyMovingBuilding == null)
        {
            return;
        }

        RestoreBuildingToOriginalPosition();
        currentlyMovingBuilding = null;
    }

    private void RestoreBuildingToOriginalPosition()
    {
        if (currentlyMovingBuilding == null) return;

        Vector3 worldPosition = GridCity.Instance.Grid.CellToWorld(new Vector3Int(originalGridPosition.x, originalGridPosition.y, 0));
        currentlyMovingBuilding.transform.position = worldPosition;

        currentlyMovingBuilding.SetGridPosition(originalGridPosition);

        for (int x = originalGridPosition.x; x < originalGridPosition.x + currentlyMovingBuilding.Size.x; x++)
        {
            for (int y = originalGridPosition.y; y < originalGridPosition.y + currentlyMovingBuilding.Size.y; y++)
            {
                GridCity.Instance.Buildings[new Vector2Int(x, y)] = currentlyMovingBuilding;
            }
        }

        ActivateCurrentlyMovingBuilding();
    }

    public void DisactivateCurrentlyMovingBuilding()
    {
        if (currentlyMovingBuilding == null) return;

        currentlyMovingBuilding.GetComponent <SpriteRenderer>().enabled = false;
        currentlyMovingBuilding.GetComponent<BoxCollider2D>().enabled = false;
    }

    public void ActivateCurrentlyMovingBuilding()
    {
        if (currentlyMovingBuilding == null) return;

        currentlyMovingBuilding.GetComponent<SpriteRenderer>().enabled = true;
        currentlyMovingBuilding.GetComponent<SpriteRenderer>().enabled = true;
    }
}
