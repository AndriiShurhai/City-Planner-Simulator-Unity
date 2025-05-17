using UnityEngine;
using Unity;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;


[System.Serializable]

public class SaveData
{
    public int money;
    public List<GameObject> residents;
    public List<Building> buildings;

    public List<Vector3Int> smallObstacles;
    public List<Vector3Int> middleObstacles;
    public List<Vector3Int> largeObstacles;

    public int currentHour;
    public int currentDay;
    public int currentMonth;
    public int currentYear;

    public float crimeRate;
    public float educationRate;
    public float happinessRate;
    public float populationRate;
    public float unemploymentRate;
}

[Serializable]
public class ZoneSaveData
{
    public string zoneName;
    public SerializableColor zoneColor;
    public List<Vector2Int> positions;
    public List<BuildingType> zoneTypes;
}

[Serializable]
public struct SerializableColor
{
    public float r, g, b, a;
    public SerializableColor(Color c)
    {
        r = c.r; g = c.g; b = c.b; a = c.a;
    }
    public Color ToColor() => new Color(r, g, b, a);
}

