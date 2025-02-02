using UnityEngine;

public class Flat : Building
{
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Initialize(BuildingData buildingData, Vector2Int size)
    {
        base.Initialize(buildingData, size);
    }
}
