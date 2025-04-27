using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnPlayButton()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnOptionsButton()
    {
    }

    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
