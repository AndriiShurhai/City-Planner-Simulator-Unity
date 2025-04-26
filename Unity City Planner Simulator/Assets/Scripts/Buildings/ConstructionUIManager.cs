using System.Collections.Generic;
using UnityEngine;

public class ConstructionUIManager : MonoBehaviour
{
    public static ConstructionUIManager Instance { get; private set; }

    [SerializeField] private GameObject constructionUIPrefab;
    private readonly Dictionary<Building, ConstructionUI> activeUIs = new Dictionary<Building, ConstructionUI>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Building.OnBuildingConstruction += RegisterBuilding;
    }

    private void OnDestroy()
    {
        Building.OnBuildingPlaced -= RegisterBuilding;
        foreach (var ui in activeUIs)
        {
            if (ui.Value != null) ui.Value.OnBuildingDestroyed();
        }
    }
    private void RegisterBuilding(Building building)
    {
        building.OnStateChanged += state => HandleStateChange(building, state);
        Building.OnBuildingDestroyed += b => RemoveUI(b);
        if (building.State == BuildingState.Constructing)
        {
            CreateUI(building);
        }
    }

    private void HandleStateChange(Building building, BuildingState state)
    {
        if (state == BuildingState.Constructing)
        {
            CreateUI(building);
        }
        else
        {
            RemoveUI(building);
        }
    }

    private void CreateUI(Building building)
    {
        Debug.Log("Creating UI");
        if (activeUIs.ContainsKey(building) || constructionUIPrefab == null)
        {
            Debug.LogWarning($"UI already exists for {building.BuildingData.buildingName} or prefab is missing.");
            return;
        }

        var uiInstance = Instantiate(constructionUIPrefab, building.transform.position + Vector3.up * 1f, Quaternion.identity);
        var constructionUI = uiInstance.AddComponent<ConstructionUI>();
        constructionUI.Initialize(building);
        activeUIs[building] = constructionUI;
    }

    private void RemoveUI(Building building)
    {
        if (activeUIs.TryGetValue(building, out var ui) && ui != null)
        {
            ui.OnBuildingDestroyed();
            activeUIs.Remove(building);
        }
    }
}