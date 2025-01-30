using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleRemover : MonoBehaviour
{
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private GameObject removeEffectPrefab;
    [SerializeField] private int removalCost = 100;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = obstacleTilemap.WorldToCell(mousePosition);

            if (obstacleTilemap.GetTile(cellPosition) != null)
            {
                obstacleTilemap.SetTile(cellPosition, null);

                // Spawn particle effect at the tile's position
                Vector3 worldPosition = obstacleTilemap.GetCellCenterWorld(cellPosition);
                Debug.Log("Spawning effect at: " + worldPosition);
                Instantiate(removeEffectPrefab, worldPosition, Quaternion.identity);
            }
        }
    }
}
