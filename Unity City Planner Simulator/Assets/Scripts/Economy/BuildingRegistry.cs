using UnityEngine;
using System.Collections.Generic;

public class BuildingRegistry : MonoBehaviour, IBuildingRegistry
{
    private readonly List<Building> _registeredBuildings = new List<Building>();
    private readonly List<GameObject> _registeredResidents = new List<GameObject>();


    public void RegisterBuilding(Building building)
    {
        if (!_registeredBuildings.Contains(building))
        {
            _registeredBuildings.Add(building);
        }
    }

    public void UnregisterBuilding(Building building)
    {
        if (_registeredBuildings.Contains(building))
        {
            _registeredBuildings.Remove(building);
        }
    }

    public int GetBuildingCount()
    {
        return _registeredBuildings.Count;
    }

    public IReadOnlyList<Building> GetAllBuildings()
    {
        return _registeredBuildings.AsReadOnly();
    }

    public void RegisterResident(GameObject resident)
    {
        if (!_registeredResidents.Contains(resident))
        {
            _registeredResidents.Add(resident);
        }
    }

    public void UnregisterResident(GameObject resident)
    {
        if (_registeredResidents.Contains(resident))
        {
            _registeredResidents.Remove(resident);
        }
    }

    public int GetResidentCount()
    {
        return _registeredResidents.Count;
    }

    public IReadOnlyList<GameObject> GetAllResidents()
    {
        return _registeredResidents.AsReadOnly();
    }
}
