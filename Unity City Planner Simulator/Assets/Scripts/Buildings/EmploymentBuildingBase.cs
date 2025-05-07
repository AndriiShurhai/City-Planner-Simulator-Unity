using UnityEngine;

public class EmploymentBuildingBase : Building, IEmploymentProvider, IZonable
{
    [SerializeField] protected int jobsAvailable = 5;

    
    public int GetAvailableJobs()
    {
        return jobsAvailable;   
    }
}
