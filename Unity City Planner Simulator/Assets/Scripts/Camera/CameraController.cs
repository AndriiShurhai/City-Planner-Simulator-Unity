using UnityEngine;
using UnityEngine.Tilemaps;

namespace SVS
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField, Tooltip("Base movement speed in units/second")]
        private float moveSpeed = 20f;
        [SerializeField, Tooltip("Screen edge percentage for scrolling")]
        private float edgeScrollThreshold = 20f;
        [SerializeField]
        private bool enableEdgeScrolling = true;

        [Header("Zoom Settings")]
        [SerializeField, Tooltip("Zoom speed multiplier")]
        private float zoomSpeed = 4f;
        [SerializeField, Tooltip("Smoothing factor for zoom transitions")]
        private float zoomSmoothness = 10f;
        [SerializeField, Tooltip("Minimum orthographic size")]
        private float minZoom = 2f;
        [SerializeField, Tooltip("Maximum orthographic size")]
        private float maxZoom = 15f;

        [Header("Map Bounds")]
        [SerializeField]
        private bool enableBounds = true;
        [SerializeField] private Tilemap groundTilemap;
        private Vector2 mapSize;

        [SerializeField]private Camera mainCamera;
        private float targetZoom;
        private float zoomVelocity;
        private bool isDragging;
        private Vector3 lastMousePosition;

        public static CameraController Instance { get; private set; }

        private void Awake()
        {
            mainCamera = Camera.main;
            Instance = this;
        }

        private void Start()
        {
            Bounds bounds = groundTilemap.localBounds;
            Debug.Log($"Bounds min: {bounds.min}, max: {bounds.max}");

            mapSize = new Vector2(bounds.size.x, bounds.size.y);

            InitializeZoom();
        }


        private void InitializeZoom()
        {
            targetZoom = Mathf.Clamp(mapSize.y / 4f, minZoom, maxZoom);
            mainCamera.orthographicSize = targetZoom;
        }

        private void Update()
        {
            HandleMovement();
            HandleZoom();
            HandleMouseDrag();
            if (enableBounds) ClampPosition();
        }

        private void HandleMovement()
        {
            Vector3 direction = GetKeyboardInput() + GetEdgeScrollInput();
            direction.Normalize(); 
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        private Vector3 GetKeyboardInput()
        {
            return new Vector3(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                0f
            );
        }

        private Vector3 GetEdgeScrollInput()
        {
            if (!enableEdgeScrolling) return Vector3.zero;

            Vector3 input = Vector3.zero;
            Vector3 mousePos = Input.mousePosition;

            // Convert threshold percentage to actual screen pixels
            input.x = GetEdgeValue(mousePos.x, Screen.width);
            input.y = GetEdgeValue(mousePos.y, Screen.height);

            return input;
        }

        private float GetEdgeValue(float position, float screenDimension)
        {
            float threshold = screenDimension * (edgeScrollThreshold / 100f);
            if (position < threshold) return -1f;
            if (position > screenDimension - threshold) return 1f;
            return 0f;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed, minZoom, maxZoom);

            mainCamera.orthographicSize = Mathf.SmoothDamp(
                mainCamera.orthographicSize,
                targetZoom,
                ref zoomVelocity,
                Time.deltaTime * zoomSmoothness
            );
        }

        private void HandleMouseDrag()
        {
            if (Input.GetMouseButtonDown(2)) // Middle mouse
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(2)) isDragging = false;

            if (isDragging)
            {
                Vector3 delta = (Input.mousePosition - lastMousePosition) * moveSpeed * Time.deltaTime;
                transform.position -= new Vector3(delta.x, delta.y, 0);
                lastMousePosition = Input.mousePosition;
            }
        }

        private void ClampPosition()
        {
            Vector3 pos = transform.position;
            float vertExtent = mainCamera.orthographicSize;
            float horizExtent = vertExtent * mainCamera.aspect;

            Bounds bounds = groundTilemap.localBounds;

            float minX = bounds.min.x + horizExtent;
            float maxX = bounds.max.x - horizExtent;
            float minY = bounds.min.y + vertExtent;
            float maxY = bounds.max.y - vertExtent;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            transform.position = pos;
        }
    }
}