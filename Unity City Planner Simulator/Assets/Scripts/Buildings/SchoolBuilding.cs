using UnityEngine;

public class SchoolBuilding : EducationBuildingBase
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        applicationsCapacity = 5;
    }

    protected override void OnUpgraded()
    {
        base.OnUpgraded();
        applicationsCapacity += 5;
    }
}
