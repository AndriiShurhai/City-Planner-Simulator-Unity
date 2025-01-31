using UnityEditor;
using UnityEngine;
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
        ManageOnMouseEnter();
    }

    private void ManageOnMouseEnter()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = obstacleTilemap.WorldToCell(mousePosition);

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
