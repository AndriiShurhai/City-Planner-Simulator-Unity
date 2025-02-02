using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "City/BuildingData", order = 1)] 
public class BuildingData : ScriptableObject
{
    public string buildingName;

    public Sprite buildingSprite;
    public Transform buildingPrefab;

    public int cost;
    public int maintenanceCost;
    public int incomePerCycle;

}
