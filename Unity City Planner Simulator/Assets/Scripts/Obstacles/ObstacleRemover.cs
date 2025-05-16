using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObstacleRemover : MonoBehaviour
{
    [SerializeField] private Tilemap _obstacleTilemap;
    [SerializeField] private Tilemap _smallObstacleTilemap;
    [SerializeField] private Tilemap _middleObstacleTilemap;
    [SerializeField] private Tilemap _largeObstacleTilemap;
    [SerializeField] private RectTransform _contextMenuPanel;
    [SerializeField] private RectTransform _selectionBox;
    [SerializeField] private Button _removeButton;
    [SerializeField] private GameObject _removeEffectPrefab;
    [SerializeField] private Text _removeButtonText; 

    private Tilemap _currentHoverObstacleTilemap;
    private Tilemap _currentObstacleTilemap;

    private Vector3Int _selectedCell;
    private Vector3Int _lastHoveredCell;
    private Vector3 _startScreenPosition;

    private readonly List<Vector3Int> _selectedCells = new List<Vector3Int>();
    private Vector3Int _startDragCell;
    private Vector3Int _endDragCell;


    private bool _isHovering = false;
    private bool _isDragging = false;

    private Camera _mainCamera;

    private const int SMALL_OBSTACLE_COST = 100;
    private const int MIDDLE_OBSTACLE_COST = 250;
    private const int LARGE_OBSTACLE_COST = 500;

    public static ObstacleRemover Instance { get; private set; }

    public Tilemap LargeObstacleTilemap => _largeObstacleTilemap;
    public Tilemap MiddleObstacleTilemap => _middleObstacleTilemap;
    public Tilemap SmallObstacleTilemap => _smallObstacleTilemap;

    public List<Vector3Int> SelectedTiles { get => _selectedCells; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        _mainCamera = Camera.main;
        _contextMenuPanel.gameObject.SetActive(false);
        _selectionBox.gameObject.SetActive(false);
        _removeButton.onClick.AddListener(OnRemoveClicked);
    }

    private void OnRemoveClicked()
    {
        if (_selectedCells.Count == 0) return;

        int totalCost = CalculateTotalRemovalCost();

        if (!EconomyManager.Instance.CanAfford(totalCost)) return;

        EconomyManager.Instance.SubtractMoney(totalCost);

        foreach (var cell in _selectedCells)
        {
            Tilemap tilemap = GetTilemapForCell(cell);
            if (tilemap != null)
            {
                tilemap.SetTile(cell, null);
                Vector3 effectPosition = tilemap.GetCellCenterWorld(cell);
                Instantiate(_removeEffectPrefab, effectPosition, Quaternion.identity);
            }
        }

        AudioManager.Instance.PlayRemoveObstacleSound();
        ClearSelection();
    }

    public int CalculateTotalRemovalCost()
    {
        int totalCost = 0;

        foreach (var cell in _selectedCells)
        {
            if (_largeObstacleTilemap.HasTile(cell))
                totalCost += LARGE_OBSTACLE_COST;
            else if (_middleObstacleTilemap.HasTile(cell))
                totalCost += MIDDLE_OBSTACLE_COST;
            else if (_smallObstacleTilemap.HasTile(cell))
                totalCost += SMALL_OBSTACLE_COST;
        }

        return totalCost;
    }

    private Tilemap GetTilemapForCell(Vector3Int cell)
    {
        if (_largeObstacleTilemap.HasTile(cell))
            return _largeObstacleTilemap;
        if (_middleObstacleTilemap.HasTile(cell))
            return _middleObstacleTilemap;
        if (_smallObstacleTilemap.HasTile(cell))
            return _smallObstacleTilemap;

        return null;
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _obstacleTilemap.WorldToCell(mousePosition);

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
        {
            StartDragSelection(cellPosition);
            return;
        }

        if (_isDragging)
        {
            UpdateDragSelection();
            return;
        }

        HandleHoverEffects(cellPosition);

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleLeftClick(cellPosition);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ClearSelection();
        }
    }

    private void StartDragSelection(Vector3Int cellPosition)
    {
        _isDragging = true;
        _startDragCell = cellPosition;
        _startScreenPosition = Input.mousePosition;
        ClearSelection();
        UpdateDragSelection();
        _selectionBox.gameObject.SetActive(true);
    }

    private void UpdateDragSelection()
    {
        UpdateSelectedBox();

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            _selectionBox.gameObject.SetActive(false);
            Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _endDragCell = _obstacleTilemap.WorldToCell(mousePos);
            ApplyRectangleSelection();
        }
    }

    private void HandleHoverEffects(Vector3Int cellPosition)
    {
        bool hasObstacle = CheckAndSetObstacleOnCellPosition(cellPosition, false);

        if (cellPosition != _lastHoveredCell && hasObstacle && !_isHovering)
        {
            AudioManager.Instance.PlayHoverSound();
            _isHovering = true;
        }
        else if (!hasObstacle)
        {
            _isHovering = false;
        }

        if (cellPosition != _lastHoveredCell && _currentHoverObstacleTilemap != null)
        {
            if (_lastHoveredCell != _selectedCell && !_selectedCells.Contains(_lastHoveredCell))
            {
                _currentHoverObstacleTilemap.SetColor(_lastHoveredCell, Color.white);
            }
            _lastHoveredCell = cellPosition;
        }

        if (hasObstacle && _currentHoverObstacleTilemap != null && !_selectedCells.Contains(cellPosition))
        {
            _currentHoverObstacleTilemap.SetColor(cellPosition, Color.yellow);
        }
    }

    private void HandleLeftClick(Vector3Int cellPosition)
    {
        if (_selectedCells.Count > 0 &&
            (!CheckAndSetObstacleOnCellPosition(cellPosition, false) || !_selectedCells.Contains(cellPosition)))
        {
            ClearSelection();
        }

        if (CheckAndSetObstacleOnCellPosition(cellPosition, assign: true))
        {
            _selectedCell = cellPosition;

            _selectedCells.Clear();
            _selectedCells.Add(cellPosition);
            _currentObstacleTilemap.SetColor(cellPosition, Color.grey);

            UpdateRemovalInterface();
        }
    }

    private void UpdateRemovalInterface()
    {
        int totalCost = CalculateTotalRemovalCost();

        if (_removeButtonText != null)
        {
            _removeButtonText.text = _selectedCells.Count > 1
                ? $"Remove ({_selectedCells.Count}) - {totalCost}$"
                : $"Remove - {totalCost}$";
        }

        _contextMenuPanel.gameObject.SetActive(_selectedCells.Count > 0);
    }

    private void ClearSelection()
    {
        foreach (var cell in _selectedCells)
        {
            Tilemap tilemap = GetTilemapForCell(cell);
            if (tilemap != null)
            {
                tilemap.SetColor(cell, Color.white);
            }
        }

        _selectedCells.Clear();
        _contextMenuPanel.gameObject.SetActive(false);
    }

    private void UpdateSelectedBox()
    {
        Vector2 currentScreenPos = Input.mousePosition;

        RectTransform parentRect = _selectionBox.parent as RectTransform;
        Vector2 localStart, localCurrent;
        Camera uiCam = (parentRect.GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay)
                       ? null
                       : _mainCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, _startScreenPosition, null, out localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, currentScreenPos, null, out localCurrent);

        Vector2 size = localCurrent - localStart;
        _selectionBox.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
        _selectionBox.pivot = new Vector2(size.x >= 0 ? 0f : 1f,
                                          size.y >= 0 ? 0f : 1f);

        _selectionBox.anchoredPosition = localStart;
    }

    private void ApplyRectangleSelection()
    {
        int xMin = Mathf.Min(_startDragCell.x, _endDragCell.x);
        int xMax = Mathf.Max(_startDragCell.x, _endDragCell.x);
        int yMin = Mathf.Min(_startDragCell.y, _endDragCell.y);
        int yMax = Mathf.Max(_startDragCell.y, _endDragCell.y);

        _selectedCells.Clear();

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                var cell = new Vector3Int(x, y, _startDragCell.z);

                if (CheckAndSetObstacleOnCellPosition(cell, assign: false))
                {
                    _selectedCells.Add(cell);

                    // Mark with selection color
                    Tilemap tilemap = GetTilemapForCell(cell);
                    if (tilemap != null)
                    {
                        tilemap.SetColor(cell, Color.grey);
                    }
                }
            }
        }

        UpdateRemovalInterface();
    }

    private bool CheckAndSetObstacleOnCellPosition(Vector3Int cellPosition, bool assign)
    {
        if (_largeObstacleTilemap.GetTile(cellPosition) != null)
        {
            SetCurrentHoverObstacle(_largeObstacleTilemap);
            if (assign)
            {
                _currentObstacleTilemap = _largeObstacleTilemap;
            }
            return true;
        }
        else if (_middleObstacleTilemap.GetTile(cellPosition) != null)
        {
            SetCurrentHoverObstacle(_middleObstacleTilemap);
            if (assign)
            {
                _currentObstacleTilemap = _middleObstacleTilemap;
            }
            return true;
        }
        else if (_smallObstacleTilemap.GetTile(cellPosition) != null)
        {
            SetCurrentHoverObstacle(_smallObstacleTilemap);
            if (assign)
            {
                _currentObstacleTilemap = _smallObstacleTilemap;
            }
            return true;
        }
        return false;
    }

    public bool CheckSmallObstacle(Vector3Int cellPosition) => _smallObstacleTilemap.HasTile(cellPosition);
    public bool CheckMiddleObstacle(Vector3Int cellPosition) => _middleObstacleTilemap.HasTile(cellPosition);
    public bool CheckLargeObstacle(Vector3Int cellPosition) => _largeObstacleTilemap.HasTile(cellPosition);
    public bool CheckObstacle(Vector3Int cellPosition) => CheckAndSetObstacleOnCellPosition(cellPosition, assign: false);

    private void SetCurrentHoverObstacle(Tilemap obstacle)
    {
        _currentHoverObstacleTilemap = obstacle;
    }
}