public interface IHappinessProvider
{
    float GetHappinessContribution();
}

public interface IHealthProvider
{
    float GetHealthContribution();
    void UpdateHealthEffect();
}

public interface IPopulationProvider
{
    int GetCurrentPopulation();
    int GetMaxPopulation();
    void AddPopulation(int amount);
}

public interface IEmploymentProvider
{
    int GetAvailableJobs();
    int GetFilledJobs();
    void FillJobs();
    void VacateJobs();
}


public interface IZonable
{

}