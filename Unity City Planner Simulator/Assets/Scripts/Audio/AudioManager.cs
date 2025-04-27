using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

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

    private AudioSource[] _sfxPool;
    private int _currentSfxIndex = 0;
    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSfxPool();
        InitializeMusicSource();
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

        if (playMusicOnStart && backgroundMusicClips.Length > 0)
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
        if (clip == null) return;
        var src = GetNextSfxSource();
        src.volume = volume;
        src.PlayOneShot(clip);
    }
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _musicSource == null) return;
        _musicSource.clip = clip;
        _musicSource.volume = volume;
        _musicSource.Play();
    }
    public void PlayRandomMusic()
    {
        if (backgroundMusicClips.Length == 0) return;
        var choice = UnityEngine.Random.Range(0, backgroundMusicClips.Length);
        PlayMusic(backgroundMusicClips[choice], musicVolume);
    }
    public void StopMusic()
    {
        if (_musicSource == null) return;
        _musicSource.Stop();
    }
    public void PlayButtonPress() => PlaySound(gameSounds.buttonPress);
    public void PlayHoverSound() => PlaySound(gameSounds.hover);
    public void PlayRemoveObstacleSound() => PlaySound(gameSounds.removeObstacle);
    public void PlayGunShot() => PlaySound(gameSounds.gunShot);
    public void PlayPeopleScream() => PlaySound(gameSounds.peopleScream);
    public void PlayHeartBeat() => PlaySound(gameSounds.heartBeat);
}
