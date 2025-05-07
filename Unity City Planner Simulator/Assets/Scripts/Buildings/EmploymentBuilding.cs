public class EmploymentBuilding : EmploymentBuildingBase
{
    public int JobsAvailable { get { return jobsAvailable; } }
    protected override void OnInitialize()
    {
        base.OnInitialize();

        jobsAvailable = 5;
    }

    protected override void OnProcessTick()
    {
        base.OnProcessTick();
    }

    protected override void OnUpgraded()
    {
        base.OnUpgraded();
        jobsAvailable += 5;
    }
}