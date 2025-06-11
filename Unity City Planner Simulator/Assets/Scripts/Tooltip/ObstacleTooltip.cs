using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private string smallObstacleMessage;
    [SerializeField] private string middleObstacleMessage;
    [SerializeField] private string largeObstacleMessage;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Tilemap largeObstacleTilemap;
    [SerializeField] private Tilemap middleObstacleTilemap;
    [SerializeField] private Tilemap smallObstacleTilemap;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (GridCity.Instance.GetActiveBuildingType() != null) return;
        ManageOnMouseEnter();
    }

    private void ManageOnMouseEnter()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = obstacleTilemap.WorldToCell(mousePosition);

        if (ObstacleRemover.Instance.SelectedTiles.Count > 1)
        {
            string message = $"To remove selected obstacles you need to pay {ObstacleRemover.Instance.CalculateTotalRemovalCost()} coins";
            TooltipManager.Instance.SetAndShowTooltip(message);
            return;
        }

        if (smallObstacleTilemap.GetTile(cellPosition) != null)
        {
            TooltipManager.Instance.SetAndShowTooltip(smallObstacleMessage);
        }
        else if(middleObstacleTilemap.GetTile(cellPosition) != null)
        {
            TooltipManager.Instance.SetAndShowTooltip(middleObstacleMessage);
        }
        else if (largeObstacleTilemap.GetTile(cellPosition) != null)
        {
            TooltipManager.Instance.SetAndShowTooltip(largeObstacleMessage);
        }
        else
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
