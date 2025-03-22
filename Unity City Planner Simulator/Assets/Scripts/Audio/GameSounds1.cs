using UnityEngine;

[CreateAssetMenu(fileName = "GameSounds1", menuName = "Audio/Game Sounds")]
public class GameSounds1 : ScriptableObject
{
    [Header("UI Sounds")]
    public AudioClip buttonPress;
    public AudioClip hover;

    [Header("Game Sounds")]
    public AudioClip removeObstacle;

    public AudioClip gunShot;
    public AudioClip bulletShot;
}
