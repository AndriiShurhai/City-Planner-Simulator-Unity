using UnityEngine;
using UnityEngine.EventSystems;

public class CustomBuildingCursor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    private BuildingData activeBuildingType;
    [SerializeField] private Grid grid;

    public static CustomBuildingCursor Instance {  get; private set; }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        spriteRenderer.enabled = false;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (activeBuildingType == null || GridCity.Instance == null) return;

        UpdateCursorPosition();
        UpdateCursorAppearance();
    }

    public void ToggleCursor(bool show, BuildingData buildingType = null)
    {
        if (buildingType != null)
        {
            activeBuildingType = buildingType;
        }
        else if (show && GridCity.Instance != null)
        {
            activeBuildingType = GridCity.Instance.GetActiveBuildingType();
            if (activeBuildingType == null)
            {
                activeBuildingType = BuildingMover.Instance.CurrentlyMovingBuilding.BuildingData;
                if (activeBuildingType == null)
                {
                    return;
                }
            }
        }

        spriteRenderer.enabled = show;
        Cursor.visible = !show;

        if (show && activeBuildingType != null)
        {
            spriteRenderer.sprite = activeBuildingType.Sprite;
        }
    }

    private void UpdateCursorPosition()
    {
        if (Camera.main == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3Int cellPosition = grid.WorldToCell(mousePos);
        transform.position = cellPosition;
    }

    private void UpdateCursorAppearance()
    {
        if (grid == null || activeBuildingType == null) return;

        Vector3Int gridPosition = grid.WorldToCell(transform.position);
        bool isValidPosition = GridCity.Instance.CanPlaceBuilding((Vector2Int)gridPosition, activeBuildingType.Size, activeBuildingType);

        spriteRenderer.color = isValidPosition ? validColor : invalidColor;
    }
}