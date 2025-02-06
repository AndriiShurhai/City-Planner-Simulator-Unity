using SVS;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    public static SceneManagerController Instance { get; private set; }
    void Start()
    {
        Instance = this;
    }

    public void LoadShopScene()
    {
        SceneManager.LoadScene("ShopScene", LoadSceneMode.Additive);
    }

    public void CloseShopScene()
    {
        SceneManager.UnloadSceneAsync("ShopScene");
    }
}
