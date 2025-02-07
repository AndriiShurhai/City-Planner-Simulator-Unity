using System.Collections.Generic;
using UnityEngine;
using Unity;
using System.Collections;
using UnityEngine.Tilemaps;

public class CarAgentManager : MonoBehaviour
{
    [SerializeField] private Tilemap roadTilemap;

    [SerializeField] private List<Sprite> redCarSprites;
    [SerializeField] private List<Sprite> greenCarSprites;
    [SerializeField] private List<Sprite> pinkCarSprites;

    [SerializeField] private List<Vector3Int> positions;
    [SerializeField] private GameObject carPrefab;  
    [SerializeField] private float interval = 4f;
    [SerializeField] private int maxCarsAllowed = 10;  

    private List<GameObject> activeCars = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnCars());
    }

    private IEnumerator SpawnCars()
    {
        while (true)  
        {
            if (activeCars.Count < maxCarsAllowed)
            {
                SpawnNewCar();
            }

            activeCars.RemoveAll(car => car == null);

            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnNewCar()
    {
        int randomCarSpriteIndex = Random.Range(0, 3);
        int randomStartPositionIndex = Random.Range(0, positions.Count);
        int randomEndPositionIndex = Random.Range(0, positions.Count);

        while (randomEndPositionIndex == randomStartPositionIndex)
        {
            randomEndPositionIndex = Random.Range(0, positions.Count);
        }

        List<Sprite> carSprites = randomCarSpriteIndex switch
        {
            0 => redCarSprites,
            1 => greenCarSprites,
            2 => pinkCarSprites,
            _ => redCarSprites
        };

        GameObject newCar = Instantiate(carPrefab,
            positions[randomStartPositionIndex],
            Quaternion.identity);

        SpriteRenderer spriteRenderer = newCar.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = carSprites[0];  
        }

        CarPathfinding pathfinding = newCar.GetComponent<CarPathfinding>();
        if (pathfinding != null)
        {
            pathfinding.carSprites = carSprites;
            pathfinding.roadTilemap = roadTilemap;
            pathfinding.SetDestination(positions[randomStartPositionIndex], positions[randomEndPositionIndex]);
            pathfinding.OnDestinationReached += () => OnCarReachedDestination(newCar);
        }

        activeCars.Add(newCar);
    }

    private void OnCarReachedDestination(GameObject car)
    {
        activeCars.Remove(car);
        Destroy(car);
    }
}
