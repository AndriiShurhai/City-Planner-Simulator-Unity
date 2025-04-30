using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Settings")]
    [SerializeField] private GameSounds gameSounds;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private int audioSourcePoolSize = 10;

    [Header("Music Settings")]
    [SerializeField] private AudioClip[] backgroundMusicClips;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;

    private const string SOUND_ENABLED_KEY = "SoundEnabled";
    private const string MUSIC_ENABLED_KEY = "MusicEnabled";
    private const string SOUND_VOLUME_KEY = "SoundVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private const bool DEFAULT_SOUND_ENABLED = true;
    private const bool DEFAULT_MUSIC_ENABLED = true;
    private const float DEFAULT_SOUND_VOLUME = 1f;
    private const float DEFAULT_MUSIC_VOLUME = 1f;

    private AudioSource[] _sfxPool;
    private int _currentSfxIndex = 0;
    private AudioSource _musicSource;
    private bool _soundEnabled;
    private bool _musicEnabled;
    private float _soundVolume;
    private float _musicVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSavedSettings();
        InitializeSfxPool();
        InitializeMusicSource();
    }

    private void LoadSavedSettings()
    {
        _soundEnabled = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, DEFAULT_SOUND_ENABLED ? 1 : 0) == 1;
        _musicEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, DEFAULT_MUSIC_ENABLED ? 1 : 0) == 1;
        _soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, DEFAULT_SOUND_VOLUME);
        _musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
    }

    private void InitializeSfxPool()
    {
        _sfxPool = new AudioSource[audioSourcePoolSize];
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            var obj = new GameObject($"SFX_Source_{i}");
            obj.transform.SetParent(transform);
            var src = obj.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxMixerGroup;
            src.playOnAwake = false;
            _sfxPool[i] = src;
        }
    }

    private void InitializeMusicSource()
    {
        var obj = new GameObject("Music_Source");
        obj.transform.SetParent(transform);
        _musicSource = obj.AddComponent<AudioSource>();
        _musicSource.outputAudioMixerGroup = musicMixerGroup;
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.mute = !_musicEnabled;
        _musicSource.volume = _musicVolume;

        if (playMusicOnStart && backgroundMusicClips.Length > 0 && _musicEnabled)
            PlayMusic(backgroundMusicClips[0], musicVolume);
    }

    private AudioSource GetNextSfxSource()
    {
        for (int i = 0; i < _sfxPool.Length; i++)
        {
            int idx = (_currentSfxIndex + i) % _sfxPool.Length;
            if (!_sfxPool[idx].isPlaying)
            {
                _currentSfxIndex = (idx + 1) % _sfxPool.Length;
                return _sfxPool[idx];
            }
        }
        var fallback = _sfxPool[_currentSfxIndex];
        _currentSfxIndex = (_currentSfxIndex + 1) % _sfxPool.Length;
        return fallback;
    }
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null || !_soundEnabled) return;
        var src = GetNextSfxSource();
        src.volume = _soundVolume * volume;
        src.PlayOneShot(clip);
    }
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _musicSource == null || !_musicEnabled) return;
        _musicSource.clip = clip;
        _musicSource.volume = _musicVolume * volume;
        _musicSource.Play();
    }
    public void PlayRandomMusic()
    {
        if (backgroundMusicClips.Length == 0 || !_musicEnabled) return;
        var choice = UnityEngine.Random.Range(0, backgroundMusicClips.Length);
        PlayMusic(backgroundMusicClips[choice], musicVolume);
    }
    public void StopMusic()
    {
        if (_musicSource == null) return;
        _musicSource.Stop();
    }

    public void SetSoundEnabled(bool enabled)
    {
        _soundEnabled = enabled;
        PlayerPrefs.SetInt(SOUND_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMusicEnabled(bool enabled)
    {
        _musicEnabled = enabled;
        PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, enabled ? 1 : 0); 

        if (_musicSource != null)
        {
            if (enabled && !_musicSource.isPlaying && backgroundMusicClips.Length > 0)
            {
                PlayMusic(_musicSource.clip != null ? _musicSource.clip : backgroundMusicClips[0]);
            }
            else if (!enabled && _musicSource.isPlaying)
            {
                _musicSource.Pause();
            }
        }
        PlayerPrefs.Save();
    }

    public void SetSoundVolume(float volume)
    {
        _soundVolume = volume;
        PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = volume;

        if (_musicSource != null)
        {
            _musicSource.volume = volume;
        }

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public bool IsSoundEnabled() => _soundEnabled;
    public bool IsMusicEnabled() => _musicEnabled;  
    public float GetSoundVolume() => _soundVolume;
    public float GetMusicVolume() => _musicVolume;

    public void PlayButtonPress() => PlaySound(gameSounds.buttonPress);
    public void PlayHoverSound() => PlaySound(gameSounds.hover);
    public void PlayRemoveObstacleSound() => PlaySound(gameSounds.removeObstacle);
    public void PlayGunShot() => PlaySound(gameSounds.gunShot);
    public void PlayPeopleScream() => PlaySound(gameSounds.peopleScream);
    public void PlayHeartBeat() => PlaySound(gameSounds.heartBeat);
}
