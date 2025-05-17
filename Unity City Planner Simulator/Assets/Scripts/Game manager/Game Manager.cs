using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Start()
    {
        SaveManager.Instance.LoadGame();
    }
}
