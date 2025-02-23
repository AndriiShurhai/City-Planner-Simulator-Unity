using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ObstacleRemover : MonoBehaviour
{
    [SerializeField] private Tilemap _obstacleTilemap;
    [SerializeField] private Tilemap _smallObstacleTilemap;
    [SerializeField] private Tilemap _middleObstacleTilemap;
    [SerializeField] private Tilemap _largeObstacleTilemap;
    [SerializeField] private RectTransform _contextMenuPanel;
    [SerializeField] private Button _removeButton;
    [SerializeField] private GameObject _removeEffectPrefab;
    [SerializeField] private int _removalCost;

    private Tilemap _currentHoverObstacleTilemap;
    private Tilemap _currentObstacleTilemap;

    private Vector3Int _selectedCell;
    private Vector3Int _lastHoveredCell;
    private Vector3Int _lastSelectedCell;

    private Tilemap _lastObstacleTilemap;

    private bool _isHovering = false;

    private Camera _mainCamera;

    public static ObstacleRemover Instance { get; private set; }
    
    public Tilemap LargeObstacleTilemap => _largeObstacleTilemap;

    public Tilemap MiddleObstacleTilemap => _middleObstacleTilemap;

    public Tilemap SmallObstacleTilemap => _smallObstacleTilemap;

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

        _removeButton.onClick.AddListener(OnRemoveClicked);
    }

    private void OnRemoveClicked()
    {
        if (!EconomyManager.Instance.CanAfford(_removalCost)) return;

        EconomyManager.Instance.SubtractMoney(_removalCost);
        _currentObstacleTilemap.SetTile(_selectedCell, null);
        _contextMenuPanel.gameObject.SetActive(false);
        AudioManager.Instance.PlayRemoveObstacleSound();

        Vector3 effectPosition = _currentObstacleTilemap.GetCellCenterWorld(_selectedCell);
        Instantiate(_removeEffectPrefab, effectPosition, Quaternion.identity);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _obstacleTilemap.WorldToCell(mousePosition);
        
        bool hasObstacle = CheckAndSetObstacleOnCellPosition(cellPosition, false);

        if (_lastHoveredCell != cellPosition && hasObstacle && !_isHovering)
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
            if (_lastHoveredCell != _selectedCell)
            {
              _currentHoverObstacleTilemap.SetColor(_lastHoveredCell, Color.white);
            }
            _lastHoveredCell = cellPosition;
        }


        if (hasObstacle && _currentObstacleTilemap != null && cellPosition != _lastSelectedCell)
        {             
            _currentHoverObstacleTilemap.SetColor(cellPosition, Color.yellow);     
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleLeftClick(cellPosition);

            
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ResetSelection(cellPosition);
        }
    }

    private void HandleLeftClick(Vector3Int cellPosition)
    {
        if (CheckAndSetObstacleOnCellPosition(cellPosition, assign: true))
        {
            if (_lastObstacleTilemap != null && _lastObstacleTilemap != _currentObstacleTilemap)
            {
                _lastObstacleTilemap.SetColor(_lastSelectedCell, Color.white);
            }
            else if (_lastObstacleTilemap != null && _lastObstacleTilemap == _currentObstacleTilemap)
            {
                if (cellPosition != _lastSelectedCell)
                {
                    _currentObstacleTilemap.SetColor(_lastSelectedCell, Color.white);
                }
            }

            _selectedCell = cellPosition;
            _lastSelectedCell = _selectedCell;
            _lastObstacleTilemap = _currentObstacleTilemap;

            _contextMenuPanel.gameObject.SetActive(true);
            _currentObstacleTilemap.SetColor(cellPosition, Color.grey);
        }

        else if (!CheckAndSetObstacleOnCellPosition(cellPosition, assign: false) && 
            _obstacleTilemap.GetTile(cellPosition) != null)
        {
            if (_currentObstacleTilemap == null) return;
            _currentObstacleTilemap.SetColor(cellPosition, Color.white);
            if (_lastObstacleTilemap != null)
            {
                _lastObstacleTilemap.SetColor(_lastSelectedCell, Color.white);
            }
            _contextMenuPanel.gameObject.SetActive(false);
        }
    }

    private void ResetSelection(Vector3Int cellPosition)
    {
        if (_currentObstacleTilemap != null)
        {
            _currentObstacleTilemap.SetColor(cellPosition, Color.white);
        }
        if (_lastObstacleTilemap != null)
        {
            _lastObstacleTilemap.SetColor(_lastSelectedCell, Color.white);
        }
        _contextMenuPanel.gameObject.SetActive(false);
    }

    private bool CheckAndSetObstacleOnCellPosition(Vector3Int cellPosition, bool assign)
    {
        if (_largeObstacleTilemap.GetTile(cellPosition) != null)
        {
            SetCurrentHoverObstacle(_largeObstacleTilemap);
            if (assign)
            {
                _currentObstacleTilemap = _largeObstacleTilemap;
                _removalCost = 500;
            }
            return true;
        }
        else if(_middleObstacleTilemap.GetTile(cellPosition) != null)
        {
            SetCurrentHoverObstacle(_middleObstacleTilemap);
            if (assign)
            {
                _currentObstacleTilemap = _middleObstacleTilemap;
                _removalCost = 250;
            }
            return true;
        }
        else if (_smallObstacleTilemap.GetTile(cellPosition) != null) 
        {
            SetCurrentHoverObstacle(_smallObstacleTilemap);
            if (assign)
            {
                _currentObstacleTilemap= _smallObstacleTilemap;
                _removalCost = 100;
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
