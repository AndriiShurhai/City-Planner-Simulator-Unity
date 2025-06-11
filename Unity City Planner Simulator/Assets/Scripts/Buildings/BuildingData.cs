using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "BuildingData", menuName = "City/BuildingData", order = 1)]
public class BuildingData : ScriptableObject
{
    [SerializeField] private string _buildingName;
    [TextArea(3, 10)]
    [SerializeField] private string _buildingDescription;
    [SerializeField] private Sprite _buildingSprite;
    [SerializeField] private Transform _buildingPrefab;
    [SerializeField] private BuildingType _buildingType;
    [SerializeField] private Vector2Int _size;
    [SerializeField] private int _cost;
    [SerializeField] private int _maintenanceCost;
    [SerializeField] private int _constructionDuration;
    [SerializeField] private int _incomePerCycle;
    [SerializeField] private int _incomePerResident;
    [SerializeField] private int _jobsAvailable;
    [SerializeField] private int _maxUpgradeLevel;
    [SerializeField] private int _upgradeCost;
    public string BuildingName => _buildingName;
    public string Description => _buildingDescription;
    public Sprite Sprite => _buildingSprite;
    public Transform Prefab => _buildingPrefab;
    public BuildingType Type => _buildingType;
    public Vector2Int Size 
    {
        get => _size; 
        private set => _size = value;
    }
    public int Cost 
    { 
        get => _cost; 
        set => _cost = value >= 0 ? value : 0; 
    }
    public int MaintenanceCost 
    { 
        get => _maintenanceCost; 
        set => _maintenanceCost = value >= 0 ? value : 0; 
    }
    public int ConstructionDuration 
    { 
        get => _constructionDuration; 
        set => _constructionDuration = value >= 0 ? value : 0;
    }
    public int IncomePerCycle 
    { 
        get => _incomePerCycle; 
        set => _incomePerCycle = value >= 0 ? value : 0; 
    }
    public int IncomePerResident 
    { 
        get => _incomePerResident; 
        set => _incomePerResident = value >= 0 ? value : 0; 
    }
    public int JobsAvailable 
    { 
        get => _jobsAvailable; 
        set => _jobsAvailable = value >= 0 ? value : 0;
    }
    public int MaxUpgradeLevel 
    { 
        get => _maxUpgradeLevel; 
        set => _maxUpgradeLevel = value >= 0 ? value : 0; 
    }
    public int UpgradeCost 
    { 
        get => _upgradeCost; 
        set => _upgradeCost = value >= 0 ? value : 0; 
    }

    protected virtual void OnValidate()
    {
        _size.x = Mathf.Max(1, _size.x);
        _size.y = Mathf.Max(1, _size.y);
        _cost = Mathf.Max(0, _cost);
        _maintenanceCost = Mathf.Max(0, _maintenanceCost);
        _constructionDuration = Mathf.Max(0, _constructionDuration);
        _incomePerCycle = Mathf.Max(0, _incomePerCycle);
        _incomePerResident = Mathf.Max(0, _incomePerResident);
        _jobsAvailable = Mathf.Max(0, _jobsAvailable);
        _maxUpgradeLevel = Mathf.Clamp(_maxUpgradeLevel, 1, 100);
        _upgradeCost = Mathf.Max(0, _upgradeCost);

        Validate();
    }
    protected virtual void Validate() { }
}