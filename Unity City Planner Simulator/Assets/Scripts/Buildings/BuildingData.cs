using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "City/BuildingData", order = 1)] 
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public string buildingDescription;

    public Sprite buildingSprite;
    public Transform buildingPrefab;
    public BuildingType buildingType;
    public Vector2Int size;
    public int cost;
    public int maintenanceCost;

    public int incomePerCycle;
    public int incomePerResident;
}
