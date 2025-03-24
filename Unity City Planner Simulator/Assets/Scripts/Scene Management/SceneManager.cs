using System;
using System.Collections.Generic;
using SVS;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    public GameObject mainScene;
    public static SceneManagerController Instance { get; private set; }

    private void Awake()
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
    void Start()
    {
    }

    public void CloseCurrentAdditiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        string mainSceneName = mainScene.scene.name;

        if (activeScene.name != mainSceneName)
        {
            FadeManager.Instance?.FadeIn();
            SceneManager.UnloadSceneAsync(activeScene.name);
        }
        else
        {
            Debug.LogWarning("The active scene is the main scene. No additive scene to unload.");
        }
    }


    public void LoadShopScene()
    {
        SceneManager.LoadScene("ShopScene", LoadSceneMode.Additive);
    }

    public void CloseShopScene()
    {
        SceneManager.UnloadSceneAsync("ShopScene");
    }

    public void LoadRobbingScene()
    {
        Debug.Log("HEEEEEEEEEEEELLLLOOOOOOOOOOOOOOO");
        Debug.Log("HELLLOOOOOOOOOOOOOOOOOOOOOOOOO");
        if (mainScene != null)
        {
            mainScene.SetActive(false);
        }
        SceneManager.LoadScene("Robbing", LoadSceneMode.Additive);
    }

    public void CloseRobbingScene()
    {
        if (mainScene != null)
        {
            mainScene.SetActive(true);
        }
        FadeManager.Instance.FadeIn();
        SceneManager.UnloadSceneAsync("Robbing");
    }

    internal void LoadMedicalAttackScene()
    {
        if (mainScene != null)
        {
            mainScene.SetActive(false);
        }
        SceneManager.LoadScene("MedicalAttackScene", LoadSceneMode.Additive);
    }

    public void CloseDialougeScene(string name)
    {
        if (mainScene != null)
        {
            mainScene.SetActive(true);
        }
        FadeManager.Instance.FadeIn();
        SceneManager.UnloadSceneAsync(name);
    }

    public void CloseMedicalAttackScene()
    {
        if (mainScene != null)
        {
            mainScene.SetActive(true);
        }
        FadeManager.Instance.FadeIn();

        Scene scene = SceneManager.GetSceneByName("MedicalAttackScene");
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync("MedicalAttackScene");
        }
        else
        {
            Debug.LogWarning("MedicalAttackScene is not loaded!");
        }
    }
}
