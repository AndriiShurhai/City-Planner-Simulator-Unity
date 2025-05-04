using System.Collections;
using UnityEngine;

public class Flat : ResidentialBuildingBase, IZonable
{
    protected override int GetInitialResidentCount()
    {
        return 5;
    }

    protected override int GetMaxResidentCount()
    {
        return 10 + buildingData.upgradeLevel * 5;
    }

    protected override void OnProcessTick()
    {
        base.OnProcessTick();

        // something else
    }


}
