using System;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ObstacleRemover : MonoBehaviour
{
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Tilemap smallObstacleTilemap;
    [SerializeField] private Tilemap middleObstacleTilemap;
    [SerializeField] private Tilemap largeObstacleTilemap;
    [SerializeField] private RectTransform contextMenuPanel;
    [SerializeField] private Button removeButton;
    [SerializeField] private GameObject removeEffectPrefab;
    [SerializeField] private int removalCost;

    private Tilemap currentObstacleTilemap;
    private Vector3Int selectedCell;
    private Vector3Int lastHoveredCell;
    private bool isHovering = false;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        contextMenuPanel.gameObject.SetActive(false);

        removeButton.onClick.AddListener(OnRemoveClicked);
    }

    private void OnRemoveClicked()
    {
        currentObstacleTilemap.SetTile(selectedCell, null);
        contextMenuPanel.gameObject.SetActive(false);
        AudioManager.Instance.PlayRemoveObstacleSound();
        Instantiate(removeEffectPrefab, currentObstacleTilemap.GetCellCenterWorld(selectedCell), Quaternion.identity);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = obstacleTilemap.WorldToCell(mousePosition);
        bool hasObstacle = CheckObstaclesOnCellPosition(cellPosition);

        if (lastHoveredCell != cellPosition && hasObstacle && !isHovering)
        {
            AudioManager.Instance.PlayHoverSound();
            isHovering = true;
        }
        else if (!hasObstacle)
        {
            isHovering = false;
        }

        if (cellPosition != lastHoveredCell && currentObstacleTilemap != null)
        {
            currentObstacleTilemap.SetColor(lastHoveredCell, Color.white);
            lastHoveredCell = cellPosition;
        }


        if (hasObstacle)
        {
            if (currentObstacleTilemap!= null) currentObstacleTilemap.SetColor(cellPosition, Color.yellow);
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (hasObstacle)
            {
                contextMenuPanel.gameObject.SetActive(true);
                selectedCell = cellPosition;
            }
        }
    }

    private bool CheckObstaclesOnCellPosition(Vector3Int cellPosition)
    {
        if (largeObstacleTilemap.GetTile(cellPosition) != null)
        {
            currentObstacleTilemap = largeObstacleTilemap;
            removalCost = 500;
            Debug.Log("The Removal Cost is 500 coins");
            return true;
        }
        else if(middleObstacleTilemap.GetTile(cellPosition) != null)
        {
            currentObstacleTilemap = middleObstacleTilemap;
            removalCost = 250;
            Debug.Log("The Removal Cost is 250 coins");
            return true;
        }
        else if (smallObstacleTilemap.GetTile(cellPosition) != null) 
        {
            currentObstacleTilemap = smallObstacleTilemap;
            removalCost = 100;
            Debug.Log("The Removal Cost is 100 coins");
            return true;
        }
        else
        {
            return false;
        }
    }
}
