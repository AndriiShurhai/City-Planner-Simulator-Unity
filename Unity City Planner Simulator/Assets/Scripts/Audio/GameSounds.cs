using UnityEngine;

[CreateAssetMenu(fileName = "GameSounds", menuName = "Audio/Game Sounds")]
public class GameSounds : ScriptableObject
{
    [Header("UI Sounds")]
    public AudioClip buttonPress;
    public AudioClip hover;

    [Header("Game Sounds")]
    public AudioClip removeObstacle;

    public AudioClip gunShot;
    public AudioClip peopleScream;
}
