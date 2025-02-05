using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip pressedButtonSound;

    // for obstacle remover
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip removeObstacleSound;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }

    public void PlayHoverSound()
    {
        AudioSource.PlayClipAtPoint(hoverSound, Camera.main.transform.position);
    }

    public void PlayRemoveObstacleSound()
    {
        AudioSource.PlayClipAtPoint(removeObstacleSound, Camera.main.transform.position);
    }
}   
