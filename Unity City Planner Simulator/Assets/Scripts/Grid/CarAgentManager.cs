using System.Collections.Generic;
using UnityEngine;
using Unity;
using System.Collections;
using UnityEngine.Tilemaps;

public class CarAgentManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private List<GameObject> positions;

    [Header("Car sprites")]
    [SerializeField] private List<Sprite> redCarSprites;
    [SerializeField] private List<Sprite> greenCarSprites;
    [SerializeField] private List<Sprite> pinkCarSprites;
    [SerializeField] private List<Sprite> blueNewCarSprites;
    [SerializeField] private List<Sprite> pinkNewCarSprites;
    [SerializeField] private List<Sprite> skyNewCarSprites;
    [SerializeField] private List<Sprite> greenNewCarSprites;

    [Header("Settings")]
    [SerializeField] private float interval = 5f;
    [SerializeField] private int maxCarsAllowed = 100;  

    private List<GameObject> activeCars = new List<GameObject>();
    private Coroutine spawnCoroutine;

    private void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnCars());
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
    private IEnumerator SpawnCars()
    {
        while (true)  
        {
            CleanupDestroyedCars();
            if (activeCars.Count < maxCarsAllowed)
            {
                SpawnNewCar();
            }

            activeCars.RemoveAll(car => car == null);

            yield return new WaitForSeconds(interval);
        }
    }

    private void CleanupDestroyedCars()
    {
        activeCars.RemoveAll(car => car == null);
    }

    private void SpawnNewCar()
    {
        int randomCarSpriteIndex = Random.Range(0, 7);
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
            3 => blueNewCarSprites,
            4 => pinkNewCarSprites,
            5 => skyNewCarSprites,
            6 => greenNewCarSprites,
            _ => redCarSprites
        };

        GameObject newCar = Instantiate(carPrefab,
            positions[randomStartPositionIndex].transform.position,
            Quaternion.identity);

        SpriteRenderer spriteRenderer = newCar.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = carSprites[0];  
        }

        ConfigureCar(newCar, carSprites, positions[randomStartPositionIndex].transform.position, positions[randomEndPositionIndex].transform.position);

        activeCars.Add(newCar);
    }

    private void ConfigureCar(GameObject car, List<Sprite> sprites, Vector3 startPosition, Vector3 endPosition)
    {
        SpriteRenderer spriteRenderer = car.GetComponent<SpriteRenderer>();   
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprites[0];
        }

        CarPathfinding pathfinding = car.GetComponent<CarPathfinding>();
        if (pathfinding != null)
        {
            pathfinding.carSprites = sprites;
            pathfinding.roadTilemap = roadTilemap;
            pathfinding.SetDestination(startPosition, endPosition);
            pathfinding.OnDestinationReached += () => OnCarReachedDestination(car);
        }
    }

    private void OnCarReachedDestination(GameObject car)
    {
        activeCars.Remove(car);
        Destroy(car);
    }
}
