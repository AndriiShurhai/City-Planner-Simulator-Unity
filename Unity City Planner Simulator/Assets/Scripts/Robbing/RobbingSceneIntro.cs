using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RobbingSceneIntro : MonoBehaviour
{
    public string robbingSceneName = "Robbing";
    public float delayBetweenShots = 1f;
    public float delayAfterShots = 1.0f;
    public AudioClip gunShots;
    public AudioClip peopleScreams;

    public void StartRobbingSequence()
    {
        StartCoroutine(GunshotsBeforeScene());
    }

    private IEnumerator GunshotsBeforeScene()
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOut();
            yield return new WaitForSeconds(FadeManager.Instance.fadeDuration);
        }

        for (int i = 0; i < 3; i++)
        {
            AudioManager.Instance.PlaySound(gunShots);
            yield return new WaitForSeconds(delayBetweenShots);
        }

        yield return new WaitForSeconds(delayAfterShots);

        AudioManager.Instance.PlaySound(peopleScreams);

        yield return new WaitForSeconds(delayAfterShots);

        SceneManagerController.Instance.LoadRobbingScene();
    }
}