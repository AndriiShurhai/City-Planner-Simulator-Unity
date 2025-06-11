using SVS;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Audio Settings UI")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [SerializeField] private Toggle edgeScrollingToggle;
    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("SettingsPanel: AudioManager instance not found!");
            return;
        }

        LoadCurrentSettings();

        if (soundToggle != null)
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);

        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);

        if (soundVolumeSlider != null)
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (edgeScrollingToggle != null)
        {
            edgeScrollingToggle.onValueChanged.AddListener(OnEdgeScrollingToggleChanged);
        }
    }

    private void OnEnable()
    {
        if (AudioManager.Instance != null)
        {
            LoadCurrentSettings();
        }
    }

    private void LoadCurrentSettings()
    {
        bool soundEnabled = AudioManager.Instance.IsSoundEnabled();
        bool musicEnabled = AudioManager.Instance.IsMusicEnabled();
        bool edgeScrollingEnabled = CameraController.Instance == null ? true : CameraController.Instance.IsEdgeScrolling();
        float soundVolume = AudioManager.Instance.GetSoundVolume();
        float musicVolume = AudioManager.Instance.GetMusicVolume();

        if (soundToggle != null)
        {
            soundToggle.SetIsOnWithoutNotify(soundEnabled);
        }

        if (musicToggle != null)
        {
            musicToggle.SetIsOnWithoutNotify(musicEnabled);
        }

        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.SetValueWithoutNotify(soundVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
        }

        if (edgeScrollingToggle != null)
        {
            edgeScrollingToggle.SetIsOnWithoutNotify(edgeScrollingEnabled);
        }
    }

    private void OnSoundToggleChanged(bool isEnabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundEnabled(isEnabled);

            if (isEnabled)
                AudioManager.Instance.PlayButtonPress();
        }
    }

    private void OnMusicToggleChanged(bool isEnabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicEnabled(isEnabled);

            if (AudioManager.Instance.IsSoundEnabled())
                AudioManager.Instance.PlayButtonPress();
        }
    }

    private void OnSoundVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundVolume(volume);
        }
    }

    private void OnMusicVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
    }

    private void OnEdgeScrollingToggleChanged(bool isEnabled)
    {
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetEdgeScrolling(isEnabled);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        LoadCurrentSettings(); 

        if (AudioManager.Instance != null && AudioManager.Instance.IsSoundEnabled())
            AudioManager.Instance.PlayButtonPress();
    }

    public void Close()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.IsSoundEnabled())
            AudioManager.Instance.PlayButtonPress();

        gameObject.SetActive(false);
    }
}