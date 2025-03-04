using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResidentsManager : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap roadTilemap;

    [SerializeField] private List<GameObject> citizensPrefabs = new List<GameObject>();

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
            int randomCitizenSpriteIndex = Random.Range(0, citizensPrefabs.Count - 1);

            GameObject prefabCitizen = citizensPrefabs[randomCitizenSpriteIndex];

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

            EconomyManager.Instance.registeredResidents.Add(newCitizen);
        }
    }
}
