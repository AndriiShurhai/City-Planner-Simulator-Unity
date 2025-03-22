using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;
    public Image fadePanel;
    public float fadeDuration = 1.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(1f, 0f, onComplete));
    }

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, System.Action onComplete)
    {
        fadePanel.gameObject.SetActive(true);
        Color panelColor = fadePanel.color;
        panelColor.a = startAlpha;
        fadePanel.color = panelColor;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;

            panelColor.a = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            fadePanel.color = panelColor;

            yield return null;
        }

        panelColor.a = targetAlpha;
        fadePanel.color = panelColor;

        if (targetAlpha == 0f)
        {
            fadePanel.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
    }
}