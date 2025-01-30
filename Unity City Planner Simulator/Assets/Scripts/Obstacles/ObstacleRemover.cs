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
    [SerializeField] private RectTransform contextMenuPanel;
    [SerializeField] private Button removeButton;
    [SerializeField] private GameObject removeEffectPrefab;
    [SerializeField] private int removalCost = 100;

    private Vector3Int selectedCell;
    private Vector3Int lastHoveredCell;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        contextMenuPanel.gameObject.SetActive(false);

        removeButton.onClick.AddListener(OnRemoveClicked);
    }

    private void OnRemoveClicked()
    {
        obstacleTilemap.SetTile(selectedCell, null);
        contextMenuPanel.gameObject.SetActive(false);
        AudioManager.Instance.PlayRemoveObstacleSound();
        Instantiate(removeEffectPrefab, obstacleTilemap.GetCellCenterWorld(selectedCell), Quaternion.identity);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = obstacleTilemap.WorldToCell(mousePosition);

        if (cellPosition != lastHoveredCell && obstacleTilemap.GetTile(cellPosition) != null)
        {
            AudioManager.Instance.PlayHoverSound();
        }

        if (cellPosition != lastHoveredCell)
        {
            obstacleTilemap.SetColor(lastHoveredCell, Color.white);
            lastHoveredCell = cellPosition;
        }


        if (obstacleTilemap.GetTile(cellPosition) != null)
        {
            obstacleTilemap.SetColor(cellPosition, Color.yellow);
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (obstacleTilemap.GetTile(cellPosition) != null)
            {
                contextMenuPanel.gameObject.SetActive(true);
                Vector2 screenPosition = Input.mousePosition;
                screenPosition = new Vector2(screenPosition.x, screenPosition.y - 240);
                contextMenuPanel.anchoredPosition = screenPosition;
                selectedCell = cellPosition;
            }
        }
    }
}
