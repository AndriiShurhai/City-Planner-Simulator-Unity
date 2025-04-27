using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "BuildingData", menuName = "City/BuildingData", order = 1)] 
public class BuildingData : ScriptableObject
{
    public string buildingName;
    [TextArea(3, 10)]
    public string buildingDescription;

    public Sprite buildingSprite;
    public Transform buildingPrefab;
    public BuildingType buildingType;
    public Vector2Int size;
    public int cost;
    public int maintenanceCost;
    public int constructionDuration;

    public int incomePerCycle;
    public int incomePerResident;

    public int jobsAvailiable;
    public int maxUpgradeLevel;
    public int upgradeLevel;
    public int upgradeCost;
}
