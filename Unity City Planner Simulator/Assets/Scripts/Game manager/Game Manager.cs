using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Start()
    {
        //SaveManager.Instance.LoadGame();
    }
}
