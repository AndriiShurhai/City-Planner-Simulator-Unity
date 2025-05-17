using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ObstacleGenerator : MonoBehaviour, ISaveable
{

    [Header("Obstacles Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap smallObstacleTilemap;
    [SerializeField] private Tilemap middleObstacleTilemap;
    [SerializeField] private Tilemap largeObstacleTilemap;

    [Header("Tilemaps where obstacles can't be")]
    [SerializeField] private Tilemap[] forbiddenTilemaps;

    [Header("Tiles")]
    [SerializeField] private AnimatedTile smallObstacleTile;
    [SerializeField] private AnimatedTile middleObstacleTile;
    [SerializeField] private AnimatedTile largeObstacleTile;

    [Header("Generation Settings")]
    [Range(0f, 1f)][SerializeField] private float smallObstacleDensity = 0.05f;
    [Range(0f, 1f)][SerializeField] private float middleObstacleDensity = 0.03f;
    [Range(0f, 1f)][SerializeField] private float largeObstacleDensity = 0.01f;

    public List<Vector3Int> smallObstaclesPositions = new List<Vector3Int>();
    public List<Vector3Int> middleObstaclePositions = new List<Vector3Int>();
    public List<Vector3Int> largeObstaclePositions = new List<Vector3Int>();

    private bool isLoaded = false;

    public static ObstacleGenerator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
        SaveManager.Instance.Register(this);
    }

    private void Start()
    {
        if (isLoaded) return;
        GenerateObstacles(smallObstacleTilemap, smallObstacleTile, smallObstacleDensity, smallObstaclesPositions);
        GenerateObstacles(middleObstacleTilemap, middleObstacleTile, middleObstacleDensity, middleObstaclePositions);
        GenerateObstacles(largeObstacleTilemap, largeObstacleTile, largeObstacleDensity, largeObstaclePositions);
    }

    private void GenerateObstacles(Tilemap map, AnimatedTile tile, float density, List<Vector3Int> obstacles)
    {
        var bounds = groundTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                if (map.HasTile(cellPos)) continue;
                if (!IsValidCell(cellPos)) continue;

                if (Random.value < density)
                {
                    map.SetTile(cellPos, tile);
                    obstacles.Add(cellPos);
                }
            }
        }
    }

    private bool IsValidCell(Vector3Int cellPosition)
    {
        foreach (var map in forbiddenTilemaps)
        {
            if (map.HasTile(cellPosition)) return false;
        }

        if (smallObstacleTilemap.HasTile(cellPosition)) return false;
        if (middleObstacleTilemap.HasTile(cellPosition)) return false;
        if (largeObstacleTilemap.HasTile(cellPosition)) return false;

        Vector3Int[] directions = { new Vector3Int(1, 0), new Vector3Int(0, 1), new Vector3Int(0, -1), new Vector3Int(-1, 0) };

        foreach(var dir in directions)
        {
            var newCell = cellPosition + dir;
            
            if (smallObstacleTilemap.HasTile(newCell)) return false;
            if (middleObstacleTilemap.HasTile(newCell)) return false;
            if (largeObstacleTilemap.HasTile(newCell)) return false;
        }

        return true;
    }

    public void Save(SaveData data)
    {
        data.smallObstacles = (smallObstaclesPositions);
        data.middleObstacles = (middleObstaclePositions);
        data.largeObstacles = (largeObstaclePositions);
    }


    public void Load(SaveData data)
    {
        smallObstacleTilemap.ClearAllTiles();
        middleObstacleTilemap.ClearAllTiles();
        largeObstacleTilemap.ClearAllTiles();

        smallObstaclesPositions.Clear();
        middleObstaclePositions.Clear();
        largeObstaclePositions.Clear();

        foreach (var pos in data.smallObstacles)
        {
            smallObstacleTilemap.SetTile(pos, smallObstacleTile);
            smallObstaclesPositions.Add(pos);
        }
        foreach (var pos in data.middleObstacles)
        {
            middleObstacleTilemap.SetTile(pos, middleObstacleTile);
            middleObstaclePositions.Add(pos);
        }
        foreach (var pos in data.largeObstacles)
        {
            largeObstacleTilemap.SetTile(pos, largeObstacleTile);
            largeObstaclePositions.Add(pos);
        }
        isLoaded = true;
    }

}
