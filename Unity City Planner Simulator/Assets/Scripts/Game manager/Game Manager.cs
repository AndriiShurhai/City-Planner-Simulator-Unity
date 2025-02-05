using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float cycleInterval = 30f;

    public static GameManager Instance { get; private set; }

    private void Start()
    {
        StartCoroutine(EconomyCycle());
    }

    private IEnumerator EconomyCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(cycleInterval);

            foreach(Building building in EconomyManager.Instance.registeredBuildings)
            {
                building.ProcessTick();
            }
            EconomyManager.Instance.UpdateUI();
        }
    }
}
