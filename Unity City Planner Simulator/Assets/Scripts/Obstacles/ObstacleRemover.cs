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

    private Tilemap currentHoverObstacleTilemap;
    private Tilemap currentObstacleTilemap;

    private Vector3Int selectedCell;
    private Vector3Int lastHoveredCell;
    private Vector3Int lastSelectedCell;

    private Tilemap lastObstacleTilemap;

    private bool isHovering = false;

    private Camera mainCamera;

    public static ObstacleRemover Instance { get; private set; }
    
    public Tilemap LargeObstacleTilemap
    {
        get
        {
            return this.largeObstacleTilemap;
        }
    }

    public Tilemap MiddleObstacleTilemap
    {
        get
        {
            return this.middleObstacleTilemap;
        }
    }

    public Tilemap SmallObstacleTilemap
    {
        get
        {
            return this.smallObstacleTilemap;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;

        contextMenuPanel.gameObject.SetActive(false);

        removeButton.onClick.AddListener(OnRemoveClicked);

        Instance = this;

    }

    private void OnRemoveClicked()
    {
        if (!EconomyManager.Instance.CanAfford(removalCost)) return;
        EconomyManager.Instance.SubtractMoney(removalCost);
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
        bool hasObstacle = CheckAndSetObstacleOnCellPosition(cellPosition, false);

        if (lastHoveredCell != cellPosition && hasObstacle && !isHovering)
        {
            AudioManager.Instance.PlayHoverSound();
            isHovering = true;
        }
        else if (!hasObstacle)
        {
            isHovering = false;
        }

        if (cellPosition != lastHoveredCell && currentHoverObstacleTilemap != null)
        {
            if (lastHoveredCell != selectedCell)
            {
              currentHoverObstacleTilemap.SetColor(lastHoveredCell, Color.white);
            }
            lastHoveredCell = cellPosition;
        }


        if (hasObstacle)
        {
            if (currentHoverObstacleTilemap != null)
            {
                if (cellPosition != lastSelectedCell)
                {
                    currentHoverObstacleTilemap.SetColor(cellPosition, Color.yellow);
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (CheckAndSetObstacleOnCellPosition(cellPosition, true))
            {
                if (lastObstacleTilemap != null && lastObstacleTilemap != currentObstacleTilemap)
                {
                    lastObstacleTilemap.SetColor(lastSelectedCell, Color.white);
                }
                else if(lastObstacleTilemap != null && lastObstacleTilemap == currentObstacleTilemap)
                {
                    if (cellPosition != lastSelectedCell)
                    {
                        currentObstacleTilemap.SetColor(lastSelectedCell, Color.white);
                    }
                }

                selectedCell = cellPosition;
                lastSelectedCell = selectedCell;
                lastObstacleTilemap = currentObstacleTilemap;

                contextMenuPanel.gameObject.SetActive(true);
                
                currentObstacleTilemap.SetColor(cellPosition, Color.grey);
            }

            else if(!CheckAndSetObstacleOnCellPosition(cellPosition, false) && obstacleTilemap.GetTile(cellPosition) != null)
            {
                currentObstacleTilemap.SetColor(cellPosition, Color.white);
                if (lastObstacleTilemap != null)
                {
                    lastObstacleTilemap.SetColor(lastSelectedCell, Color.white);
                }
                contextMenuPanel.gameObject.SetActive(false);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (currentObstacleTilemap!= null)
            {
                currentObstacleTilemap.SetColor(cellPosition, Color.white);
            }
            if (lastObstacleTilemap != null)
            {
                lastObstacleTilemap.SetColor(lastSelectedCell, Color.white);
            }
            contextMenuPanel.gameObject.SetActive(false);
        }
    }

    private bool CheckAndSetObstacleOnCellPosition(Vector3Int cellPosition, bool assign)
    {
        if (largeObstacleTilemap.GetTile(cellPosition) != null)
        {
            SetCurrentHoverObstacle(largeObstacleTilemap);
            if (assign)
            {
                currentObstacleTilemap = largeObstacleTilemap;
                removalCost = 500;
            }
            return true;
        }
        else if(middleObstacleTilemap.GetTile(cellPosition) != null)
        {
            SetCurrentHoverObstacle(middleObstacleTilemap);
            if (assign)
            {
                currentObstacleTilemap = middleObstacleTilemap;
                removalCost = 250;
            }
            return true;
        }
        else if (smallObstacleTilemap.GetTile(cellPosition) != null) 
        {
            SetCurrentHoverObstacle(smallObstacleTilemap);
            if (assign)
            {
                currentObstacleTilemap= smallObstacleTilemap;
                removalCost = 100;
            }
            return true;
        }
        return false;
    }

    private void SetCurrentHoverObstacle(Tilemap obstacle)
    {
        currentHoverObstacleTilemap = obstacle;
    }
}
