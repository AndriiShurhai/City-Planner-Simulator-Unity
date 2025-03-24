using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalAttack : MonoBehaviour
{
    [SerializeField] private AudioClip heartBeat;
    [SerializeField] private AudioClip crying;

    public void StartMedicalAttack()
    {
        StopAllCoroutines();
        StartCoroutine(MedicalSceneSequence());
    }

    private IEnumerator MedicalSceneSequence()
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOut();
            yield return new WaitForSeconds(FadeManager.Instance.fadeDuration);
        }

        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlaySound(heartBeat);
        yield return new WaitForSeconds(heartBeat.length);

        AudioManager.Instance.PlaySound(crying);

        SceneManagerController.Instance.LoadMedicalAttackScene();
    }

}
