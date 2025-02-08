using UnityEngine;

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

        ToggleCursor(false);
    }

    private void Update()
    {
        if (activeBuildingType == null || GridCity.Instance == null) return;

        UpdateCursorPosition();
        UpdateCursorAppearance();
    }

    public void ToggleCursor(bool show, BuildingData buildingType = null)
    {
        if (GridCity.Instance == null || GridCity.Instance.GetActiveBuildingType() == null)
        {
            Cursor.visible = !show;
            spriteRenderer.enabled = show;
            return;
        }
        activeBuildingType = GridCity.Instance.GetActiveBuildingType();
        spriteRenderer.enabled = show;
        Cursor.visible = !show;

        if (show && activeBuildingType != null)
        {
            spriteRenderer.sprite = activeBuildingType.buildingSprite;
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
        bool isValidPosition = GridCity.Instance.CanPlaceBuilding((Vector2Int)gridPosition, activeBuildingType.size, activeBuildingType);

        spriteRenderer.color = isValidPosition ? validColor : invalidColor;
    }
}