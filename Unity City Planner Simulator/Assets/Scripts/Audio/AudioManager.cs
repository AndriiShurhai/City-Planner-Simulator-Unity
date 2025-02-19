using System;
using System.Diagnostics.Tracing;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameSounds gameSounds;
    [SerializeField] private AudioClip pressedButtonSound;
    [SerializeField] private int audioSourcePoolSize = 10;

    private AudioSource[] _audioSourcePool;
    private int _currentAudioSourceIndex = 0;

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        _audioSourcePool = new AudioSource[audioSourcePoolSize];

        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject audioSourceObj = new GameObject($"Audiosource_{i}");
            audioSourceObj.transform.parent = transform;
            _audioSourcePool[i] = audioSourceObj.AddComponent<AudioSource>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;

        AudioSource audioSource = GetNextAudioSource();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private AudioSource GetNextAudioSource()
    {

        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            int index = (_currentAudioSourceIndex + i) % audioSourcePoolSize;
            if (!_audioSourcePool[index].isPlaying)
            {
                _currentAudioSourceIndex = (index + 1) % audioSourcePoolSize;
                return _audioSourcePool[index];
            }
        }
        AudioSource audioSource = _audioSourcePool[_currentAudioSourceIndex];
        _currentAudioSourceIndex = (_currentAudioSourceIndex + 1) % audioSourcePoolSize;

        return audioSource;
    }

    public void PlayHoverSound()
    {
        PlaySound(gameSounds.hover);
    }

    public void PlayRemoveObstacleSound()
    {
        PlaySound(gameSounds.removeObstacle);
    }
}   
