using System.Collections.Generic;
using UnityEngine;

public class HospitalsManager : MonoBehaviour
{
    public List<Doctor> doctors;

    public static HospitalsManager Instance;

    private AIResident currentIllResident;
    private int doctorsCount;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        doctorsCount = doctors.Count;

        ResidentsManager.Instance.OnResidentGoingToDie += ProccessHealthAttackAction;
    }

    private void Update()
    {
        if (doctors.Count > doctorsCount)
        {
            if (currentIllResident != null)
            {
                ProccessHealthAttackAction(currentIllResident);
            }
            doctorsCount = doctors.Count;
        }
    }

    private void ProccessHealthAttackAction(AIResident criminal)
    {
        currentIllResident = criminal;
        float minDistance = int.MaxValue;
        Doctor nearestDoctor = null;
        foreach (var doctor in doctors)
        {
            Vector3 distance = criminal.transform.position - doctor.transform.position;
            float cells = Mathf.Abs(distance.x) + Mathf.Abs(distance.y) + Mathf.Abs(distance.z);

            if (cells < minDistance)
            {
                minDistance = cells;
                nearestDoctor = doctor;
            }
        }

        if (nearestDoctor == null)
        {
            Debug.Log("Nearest policeman is not found!!!");
            return;
        }
        nearestDoctor.GoHealCitizen(criminal.transform, criminal);
    }

    public void ResetCurrentILLResident()
    {
        currentIllResident = null;
    }
}
