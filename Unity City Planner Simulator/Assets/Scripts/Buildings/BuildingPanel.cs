using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanel : MonoBehaviour
{
    [SerializeField] private Button destroyBuildingButton;
    [SerializeField] private Button infoBuildingButton;
    [SerializeField] private Button upgradeBuildingButton;
    [SerializeField] private Button moveBuildingButton;

    private bool isShowing;
    private Vector2Int currentPosition;
    public static BuildingPanel Instance { get; private set; }


    private void Awake()
    {
        gameObject.SetActive(false);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    void Update()
    {
        if (isShowing)
        {
            transform.position = Camera.main.WorldToScreenPoint(new Vector3(currentPosition.x, currentPosition.y, 0));
        }
    }

    public void ShowBuildingPanel(Building building, Vector2Int position)
    {
        RemoveAllListeners();
        AddListeners(building);
        isShowing = true;
        currentPosition = position;
        Debug.Log(position.x + " " + position.y);

        transform.position = Camera.main.WorldToScreenPoint(new Vector3(position.x + 0.5f, position.y + 0.5f, 0));
        Debug.Log(transform.position.x + " " + transform.position.y);
        gameObject.SetActive(true);
    }

    public void HideBuildingPanel()
    {
        isShowing = false;
        gameObject.SetActive(false);
    }

    private void RemoveAllListeners()
    {
        destroyBuildingButton.onClick.RemoveAllListeners();
        infoBuildingButton.onClick.RemoveAllListeners();
        upgradeBuildingButton.onClick.RemoveAllListeners();
        moveBuildingButton.onClick.RemoveAllListeners();
    }

    private void AddListeners(Building building)
    {
        destroyBuildingButton.onClick.AddListener(building.DestroyBuilding);
        destroyBuildingButton.onClick.AddListener(HideBuildingPanel);

        infoBuildingButton.onClick.AddListener(HideBuildingPanel);

        upgradeBuildingButton.onClick.AddListener(building.Upgrade);
        upgradeBuildingButton.onClick.AddListener(HideBuildingPanel);

        moveBuildingButton.onClick.AddListener(building.MoveBuilding);
        moveBuildingButton.onClick.AddListener(HideBuildingPanel);
    }
}
