using UnityEngine;
using Unity;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using JetBrains.Annotations;


[Serializable]

public class SaveData
{
    public int money;
    public List<ResidentSaveData> residents;

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
    public float healthRate;
    public float populationRate;
    public float unemploymentRate;

    public int currentCriminalID;
    public int currentIllResidentID;

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


[Serializable]

public struct ResidentSaveData
{
    public int residentID;
    public int prefabIndex;
    public string residentType;
    public Vector3 position;
    public bool isCommittingCrime;
    public bool isHavingHeartAttack;
    public float healthTimer;
    public bool isTryingToCure;
    public int illCitizenID;
    public bool isChasing;
    public int criminalID;
    public float chasingRecalculationCooldown;
    public Vector3 currentDestination;
    public bool isMoving;
}





