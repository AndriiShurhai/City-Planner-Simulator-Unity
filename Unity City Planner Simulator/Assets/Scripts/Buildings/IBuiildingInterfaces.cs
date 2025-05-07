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
}

public interface IEducationProvider
{
    float GetEducationContribution();
}

public interface IZonable
{

}