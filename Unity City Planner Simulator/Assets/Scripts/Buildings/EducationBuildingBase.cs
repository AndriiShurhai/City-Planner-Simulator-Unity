using UnityEngine;

public class EducationBuildingBase : Building, IEducationProvider
{
    [SerializeField] protected int applicationsCapacity;
    public float GetEducationContribution()
    {
        return applicationsCapacity;
    }
}
