using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResidentsManager : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap roadTilemap;

    [SerializeField] private GameObject citizenSprite1Prefab;
    [SerializeField] private GameObject citizenSprite2Prefab;
    [SerializeField] private GameObject citizenSprite3Prefab;
    [SerializeField] private GameObject citizenSprite4Prefab;
    [SerializeField] private GameObject citizenSprite5Prefab;
    [SerializeField] private GameObject citizenSprite6Prefab;

    private List<GameObject> activeCitizens = new List<GameObject>();

    public static ResidentsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SpawnResidents(int amount, Vector3Int position)
    {
        int offset = 1;

        position = new Vector3Int(position.x - offset * (amount / 2), position.y - offset, position.z);
        for (int i = 0; i < amount; i++)
        {
            int randomCitizenSpriteIndex = Random.Range(0, 6);

            GameObject prefabCitizen = randomCitizenSpriteIndex switch
            {
                0 => citizenSprite1Prefab,
                1 => citizenSprite2Prefab,
                2 => citizenSprite3Prefab,
                3 => citizenSprite4Prefab,
                4 => citizenSprite5Prefab,
                5 => citizenSprite6Prefab,
                _ => citizenSprite6Prefab,
            };

            GameObject newCitizen = prefabCitizen;        

            if (newCitizen.GetComponent<AIResident>() == null )
            {
                newCitizen.AddComponent<AIResident>();
            }

            newCitizen.GetComponent<AIResident>().Initialize(groundTilemap, roadTilemap, newCitizen.GetComponent<Animator>());

            position = new Vector3Int(position.x + offset, position.y, position.z);

            Instantiate(newCitizen,
                position,
                Quaternion.identity
            );
        }
    }

}
