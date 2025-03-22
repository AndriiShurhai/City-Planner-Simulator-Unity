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
}
