using UnityEngine;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using JetBrains.Annotations;
using System;

public class ResidentialHouse : ResidentialBuildingBase, IZonable
{
    [SerializeField] private ParticleSystem ps;
    private bool isPlaying = false;

    public new event Action<int, Vector3> OnUpgrade;

    protected override int GetInitialResidentCount()
    {
        return 1;
    }

    protected override int GetMaxResidentCount()
    {
        return 5 + _upgradeLevel * 2;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (!isPlaying)
        {
            StartCoroutine(PlayParticlesWithDelay());
        }
    }
    private IEnumerator PlayParticlesWithDelay()
    {
        while (true)
        {
            isPlaying = true;
            float delay = UnityEngine.Random.Range(1f, 80f);
            yield return new WaitForSeconds(delay);

            ps.Play();
            isPlaying = false;
        }
    }

    protected override void OnUpgraded()
    {
        base.OnUpgraded();

        if (OnUpgrade != null && OccupiedPositions.Count > 0)
        {
            Vector3 position = new Vector3(OccupiedPositions[0].x, OccupiedPositions[0].y, 0);
            OnUpgrade.Invoke(_upgradeLevel, position);
        }
    }
}
