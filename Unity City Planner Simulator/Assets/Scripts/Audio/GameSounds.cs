using UnityEngine;

[CreateAssetMenu(fileName ="GameSounds", menuName = "Audio/Game Soudns")]
public class GameSounds : ScriptableObject
{
    [Header("UI Sounds")]
    public AudioClip buttonPress;
    public AudioClip hover;

    [Header("Game Sounds")]
    public AudioClip removeObstacle;
}
