using UnityEngine;
using System.Collections;

public class PinkHouse : Building
{
    [SerializeField] private ParticleSystem ps;
    private bool isPlaying = false;


    private void Update()
    {
        if (!isPlaying)
        {
            StartCoroutine(PlayParticlesWithDelay());
        }
    }

    private IEnumerator PlayParticlesWithDelay()
    {
        isPlaying = true;
        float delay = UnityEngine.Random.Range(1f, 5f);
        yield return new WaitForSeconds(delay);

        ps.Play();

        isPlaying = false;
    }

}
