using UnityEngine;
using System.Collections.Generic;
using System;

public class PoliceStationManager : MonoBehaviour
{
    public List<Policeman> policemans;

    public static PoliceStationManager Instance;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        ResidentsManager.Instance.OnResidentGointToCrime += ProccessCrimeAction;
    }

    private void ProccessCrimeAction(AIResident criminal)
    {
        float minDistance = int.MaxValue;
        Policeman nearestPoliceman = null;
        foreach (var policeman in policemans)
        {
            Vector3 distance = criminal.transform.position - policeman.transform.position;
            float cells = Mathf.Abs(distance.x) + Mathf.Abs(distance.y) + Mathf.Abs(distance.z);

            if (cells < minDistance)
            {
                minDistance = cells;
                nearestPoliceman = policeman;
            }
        }

        if (nearestPoliceman == null)
        {
            Debug.Log("Nearest policeman is not found!!!");
            return;
        }
        nearestPoliceman.StartChasingCriminal(criminal.transform, criminal);
    }
}
